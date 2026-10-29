using Microsoft.Extensions.Options;
using WebApi.Data.Auth.Models.V1;
using WebApi.Data.Auth.Repositories.V1;
using WebApi.Data.Auth.Repositories.V1.Base;
using WebApi.Data.Auth.Services.V1;
using WebApi.Data.Auth.Services.V1.Base;

namespace WebApi.Extensions.V1;

public static class KeycloakExtensions
{
    public static void InitKeycloak(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));

        services.AddHttpClient(KeycloakOptions.HttpClientName, (provider, client) =>
        {
            var options = provider.GetRequiredService<IOptions<KeycloakOptions>>().Value;
            client.BaseAddress = new Uri($"{options.BaseUrl.TrimEnd('/')}/");
        });

        // Singletons: the user repository caches the service account token across requests.
        services.AddSingleton<ITokenRepository, TokenKeycloakRepository>();
        services.AddSingleton<IUserRepository, UserKeycloakRepository>();

        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IUserService, UserService>();
    }
}
