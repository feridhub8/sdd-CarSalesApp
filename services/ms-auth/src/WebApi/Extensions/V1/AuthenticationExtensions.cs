using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.JsonWebTokens;
using WebApi.Data.Auth.Models.V1;
using WebApi.Data.Auth.Repositories.V1.Base;

namespace WebApi.Extensions.V1;

public static class AuthenticationExtensions
{
    public static void InitAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var keycloak = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
            ?? throw new InvalidOperationException($"The '{KeycloakOptions.SectionName}' section is missing from appsettings.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = $"{keycloak.BaseUrl.TrimEnd('/')}/realms/{keycloak.Realm}";
                options.Audience = keycloak.ClientId;
                options.RequireHttpsMetadata = keycloak.RequireHttpsMetadata;
                options.MapInboundClaims = false;

                // A signed, unexpired token is not enough: after logout its session is gone in Keycloak.
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var tokenRepository = context.HttpContext.RequestServices.GetRequiredService<ITokenRepository>();
                        var accessToken = (context.SecurityToken as JsonWebToken)?.EncodedToken;

                        if (accessToken is null || !await tokenRepository.IsActiveAsync(accessToken, context.HttpContext.RequestAborted))
                        {
                            context.Fail("The session of this access token has ended.");
                        }
                    }
                };
            });

        services.AddAuthorization();
    }
}
