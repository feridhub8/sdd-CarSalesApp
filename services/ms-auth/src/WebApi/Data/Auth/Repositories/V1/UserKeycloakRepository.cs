using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Options;
using WebApi.Data.Auth.Models.V1;
using WebApi.Data.Auth.Repositories.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Auth.Repositories.V1;

public sealed class UserKeycloakRepository : IUserRepository
{
    private static readonly TimeSpan TokenRefreshMargin = TimeSpan.FromSeconds(30);

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly IHttpClientFactory httpClientFactory;
    private readonly ITokenRepository tokenRepository;
    private readonly KeycloakOptions options;
    private readonly SemaphoreSlim tokenLock = new(1, 1);

    private string? serviceAccountToken;
    private DateTimeOffset serviceAccountTokenExpiresAt;

    public UserKeycloakRepository(
        IHttpClientFactory httpClientFactory, ITokenRepository tokenRepository, IOptions<KeycloakOptions> options)
    {
        this.httpClientFactory = httpClientFactory;
        this.tokenRepository = tokenRepository;
        this.options = options.Value;
    }

    private string UsersPath => $"admin/realms/{this.options.Realm}/users";

    public async Task<Guid> CreateAsync(KeycloakUserRepresentation user, CancellationToken ct = default)
    {
        var client = await this.CreateAuthorizedClientAsync(ct);
        using var response = await client.PostAsJsonAsync(this.UsersPath, user, JsonOptions, ct);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var conflict = await response.Content.ReadFromJsonAsync<KeycloakErrorResponse>(ct);
            throw new ConflictException(conflict?.ErrorMessage switch
            {
                { } message when message.Contains("username", StringComparison.OrdinalIgnoreCase) => "An account with this username already exists.",
                { } message when message.Contains("email", StringComparison.OrdinalIgnoreCase) => "An account with this email already exists.",
                _ => "An account with these details already exists."
            });
        }

        if (response.StatusCode == HttpStatusCode.BadRequest)
        {
            // Backstop: the validator checks the same policy, so this only fires if the two ever differ.
            var error = await response.Content.ReadFromJsonAsync<KeycloakErrorResponse>(ct);
            if (error?.Error?.StartsWith(KeycloakErrorResponse.InvalidPasswordPrefix, StringComparison.Ordinal) == true)
            {
                throw new ValidationException([new ValidationFailure("password", error.ErrorDescription ?? "The password does not meet the policy.")]);
            }
        }

        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location?.ToString()
            ?? throw new InvalidOperationException("Keycloak did not return the new user's location.");

        return Guid.Parse(location[(location.LastIndexOf('/') + 1)..]);
    }

    public async Task<KeycloakUserRepresentation?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var client = await this.CreateAuthorizedClientAsync(ct);
        using var response = await client.GetAsync($"{this.UsersPath}/{id}", ct);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<KeycloakUserRepresentation>(JsonOptions, ct);
    }

    private async Task<HttpClient> CreateAuthorizedClientAsync(CancellationToken ct)
    {
        var client = this.httpClientFactory.CreateClient(KeycloakOptions.HttpClientName);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await this.GetServiceAccountTokenAsync(ct));
        return client;
    }

    private async Task<string> GetServiceAccountTokenAsync(CancellationToken ct)
    {
        if (this.serviceAccountToken is not null && DateTimeOffset.UtcNow < this.serviceAccountTokenExpiresAt)
        {
            return this.serviceAccountToken;
        }

        await this.tokenLock.WaitAsync(ct);
        try
        {
            if (this.serviceAccountToken is null || DateTimeOffset.UtcNow >= this.serviceAccountTokenExpiresAt)
            {
                var token = await this.tokenRepository.GetServiceAccountTokenAsync(ct);
                this.serviceAccountToken = token.AccessToken;
                this.serviceAccountTokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn) - TokenRefreshMargin;
            }

            return this.serviceAccountToken;
        }
        finally
        {
            this.tokenLock.Release();
        }
    }
}
