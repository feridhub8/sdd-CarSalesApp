using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using WebApi.Data.Shared.Models.V1;
using WebApi.Data.Shared.Repositories.V1.Base;

namespace WebApi.Extensions.V1;

public static class AuthenticationExtensions
{
    private const string RoleClaimType = "role";
    private const string RealmAccessClaim = "realm_access";

    public static void InitAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloak = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
            ?? throw new InvalidOperationException($"The '{KeycloakOptions.SectionName}' section is missing from appsettings.");

        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));
        services.AddHttpClient(KeycloakOptions.HttpClientName, (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
            client.BaseAddress = new Uri($"{options.BaseUrl.TrimEnd('/')}/");
        });

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{keycloak.BaseUrl.TrimEnd('/')}/realms/{keycloak.Realm}";
                options.Audience = keycloak.ClientId;
                options.RequireHttpsMetadata = keycloak.RequireHttpsMetadata;
                options.MapInboundClaims = false;
                options.TokenValidationParameters.RoleClaimType = RoleClaimType;

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        // A signed, unexpired token is not enough: after logout its session is gone in Keycloak.
                        var tokenRepository = context.HttpContext.RequestServices.GetRequiredService<ITokenRepository>();
                        var accessToken = (context.SecurityToken as JsonWebToken)?.EncodedToken;

                        if (accessToken is null || !await tokenRepository.IsActiveAsync(accessToken, context.HttpContext.RequestAborted))
                        {
                            context.Fail("The session of this access token has ended.");
                            return;
                        }

                        AddRealmRoles(context.Principal);
                    }
                };
            });

        services.AddAuthorization();
    }

    // Keycloak puts realm roles in a JSON claim ({"roles":["admin",...]}); [Authorize(Roles = ...)] needs one claim per role.
    private static void AddRealmRoles(ClaimsPrincipal? principal)
    {
        if (principal?.Identity is not ClaimsIdentity identity || identity.FindFirst(RealmAccessClaim)?.Value is not { } realmAccess)
        {
            return;
        }

        using var document = JsonDocument.Parse(realmAccess);
        if (!document.RootElement.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
        {
            return;
        }

        foreach (var role in roles.EnumerateArray().Select(role => role.GetString()).OfType<string>())
        {
            identity.AddClaim(new Claim(RoleClaimType, role));
        }
    }
}
