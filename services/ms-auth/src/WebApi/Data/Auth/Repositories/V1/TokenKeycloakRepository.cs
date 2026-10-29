using System.Net;
using Microsoft.Extensions.Options;
using WebApi.Data.Auth.Models.V1;
using WebApi.Data.Auth.Repositories.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Auth.Repositories.V1;

public sealed class TokenKeycloakRepository : ITokenRepository
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly KeycloakOptions options;

    public TokenKeycloakRepository(IHttpClientFactory httpClientFactory, IOptions<KeycloakOptions> options)
    {
        this.httpClientFactory = httpClientFactory;
        this.options = options.Value;
    }

    private string OpenIdConnectPath => $"realms/{this.options.Realm}/protocol/openid-connect";

    public Task<KeycloakTokenResponse> GetByPasswordAsync(string login, string password, CancellationToken ct = default)
    {
        return this.RequestTokenAsync(
            [new("grant_type", "password"), new("username", login), new("password", password)],
            "Invalid login or password.",
            ct);
    }

    public Task<KeycloakTokenResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        return this.RequestTokenAsync(
            [new("grant_type", "refresh_token"), new("refresh_token", refreshToken)],
            "The refresh token is invalid or expired.",
            ct);
    }

    public Task<KeycloakTokenResponse> GetServiceAccountTokenAsync(CancellationToken ct = default)
    {
        return this.RequestTokenAsync([new("grant_type", "client_credentials")], "The ms-auth client was rejected by Keycloak.", ct);
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken ct = default)
    {
        using var response = await this.PostFormAsync($"{this.OpenIdConnectPath}/logout", [new("refresh_token", refreshToken)], ct);
        await EnsureSuccessAsync(response, "The refresh token is invalid or expired.", ct);
    }

    public async Task<bool> IsActiveAsync(string token, CancellationToken ct = default)
    {
        using var response = await this.PostFormAsync($"{this.OpenIdConnectPath}/token/introspect", [new("token", token)], ct);
        response.EnsureSuccessStatusCode();

        var introspection = await response.Content.ReadFromJsonAsync<KeycloakIntrospectionResponse>(ct);
        return introspection?.Active == true;
    }

    private async Task<KeycloakTokenResponse> RequestTokenAsync(
        List<KeyValuePair<string, string>> fields, string invalidGrantMessage, CancellationToken ct)
    {
        using var response = await this.PostFormAsync($"{this.OpenIdConnectPath}/token", fields, ct);
        await EnsureSuccessAsync(response, invalidGrantMessage, ct);

        return await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(ct)
            ?? throw new InvalidOperationException("Keycloak returned an empty token response.");
    }

    private async Task<HttpResponseMessage> PostFormAsync(string path, List<KeyValuePair<string, string>> fields, CancellationToken ct)
    {
        fields.Add(new("client_id", this.options.ClientId));
        fields.Add(new("client_secret", this.options.ClientSecret));

        var client = this.httpClientFactory.CreateClient(KeycloakOptions.HttpClientName);
        return await client.PostAsync(path, new FormUrlEncodedContent(fields), ct);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, string invalidGrantMessage, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized)
        {
            var error = await response.Content.ReadFromJsonAsync<KeycloakErrorResponse>(ct);
            if (error?.Error == KeycloakErrorResponse.InvalidGrant)
            {
                throw new UnauthorizedException(invalidGrantMessage);
            }
        }

        response.EnsureSuccessStatusCode();
    }
}
