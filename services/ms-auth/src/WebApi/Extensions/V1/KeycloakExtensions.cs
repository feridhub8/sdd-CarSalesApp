using Microsoft.Extensions.Options;
using WebApi.Data.Auth.Models.V1;

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
    }
}
