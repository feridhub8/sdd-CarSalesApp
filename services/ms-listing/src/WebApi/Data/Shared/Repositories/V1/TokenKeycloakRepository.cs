using Microsoft.Extensions.Options;
using WebApi.Data.Shared.Models.V1;
using WebApi.Data.Shared.Repositories.V1.Base;

namespace WebApi.Data.Shared.Repositories.V1;

public sealed class TokenKeycloakRepository : ITokenRepository
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly KeycloakOptions options;

    public TokenKeycloakRepository(IHttpClientFactory httpClientFactory, IOptions<KeycloakOptions> options)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
    }

    public async Task<bool> IsActiveAsync(string token, CancellationToken ct = default)
    {
        var client = this.httpClientFactory.CreateClient(KeycloakOptions.HttpClientName);
        var fields = new FormUrlEncodedContent(
        [
            new("token", token),
            new("client_id", this.options.ClientId),
            new("client_secret", this.options.ClientSecret)
        ]);

        using var response = await client.PostAsync($"realms/{this.options.Realm}/protocol/openid-connect/token/introspect", fields, ct);
        response.EnsureSuccessStatusCode();

        var introspection = await response.Content.ReadFromJsonAsync<KeycloakIntrospectionResponse>(ct);
        return introspection?.Active == true;
    }
}
