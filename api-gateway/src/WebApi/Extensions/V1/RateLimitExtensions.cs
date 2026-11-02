using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WebApi.RateLimiting.V1;

namespace WebApi.Extensions.V1;

public static class RateLimitExtensions
{
    private const string UnknownClient = "unknown";

    public static void InitRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        var limits = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
            ?? throw new InvalidOperationException($"The '{RateLimitOptions.SectionName}' section is missing from appsettings.");

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteRejectionAsync;

            // Routes pick a policy by name through RateLimiterPolicy in the ReverseProxy settings;
            // routes without one, such as the website and image files, are not limited.
            AddPolicy(options, "login", limits.Login);
            AddPolicy(options, "register", limits.Register);
            AddPolicy(options, "seller", limits.Seller);
            AddPolicy(options, "api", limits.Api);
        });
    }

    private static void AddPolicy(RateLimiterOptions options, string name, RateLimitPolicyOptions limit)
    {
        options.AddPolicy(name, context => RateLimitPartition.GetFixedWindowLimiter(
            ClientOf(context),
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = limit.PermitLimit,
                Window = TimeSpan.FromSeconds(limit.WindowSeconds),
                QueueLimit = 0
            }));
    }

    // Only the connection's address counts. X-Forwarded-For is not used, since a client could
    // send a new value with each request and never reach a limit.
    private static string ClientOf(HttpContext context)
    {
        var address = context.Connection.RemoteIpAddress;
        return address is null ? UnknownClient : Normalize(address).ToString();
    }

    private static IPAddress Normalize(IPAddress address)
    {
        return address.IsIPv4MappedToIPv6 ? address.MapToIPv4() : address;
    }

    private static async ValueTask WriteRejectionAsync(OnRejectedContext rejected, CancellationToken ct)
    {
        var response = rejected.HttpContext.Response;

        if (rejected.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            response.Headers.RetryAfter = ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();
        }

        await response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too Many Requests",
                Detail = "Too many requests. Try again later."
            },
            options: null,
            contentType: "application/problem+json",
            cancellationToken: ct);
    }
}
