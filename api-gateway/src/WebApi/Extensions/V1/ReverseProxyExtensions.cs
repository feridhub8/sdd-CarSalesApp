using System.Net.Sockets;

namespace WebApi.Extensions.V1;

public static class ReverseProxyExtensions
{
    private const string SectionName = "ReverseProxy";
    private static readonly TimeSpan DnsRefreshPeriod = TimeSpan.FromSeconds(10);

    public static void InitReverseProxy(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReverseProxy()
            .LoadFromConfig(configuration.GetSection(SectionName))
            // Expands each destination host into one destination per IP, so a scaled service gets
            // load on every replica, and re-resolves periodically to follow replicas as they come and go.
            .AddDnsDestinationResolver(options =>
            {
                options.RefreshPeriod = DnsRefreshPeriod;
                options.AddressFamily = AddressFamily.InterNetwork;
            });
    }
}
