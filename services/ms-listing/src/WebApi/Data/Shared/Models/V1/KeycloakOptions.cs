namespace WebApi.Data.Shared.Models.V1;

public sealed class KeycloakOptions
{
    public const string SectionName = "Keycloak";
    public const string HttpClientName = "Keycloak";

    public string BaseUrl { get; init; } = string.Empty;

    public string Realm { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string ClientSecret { get; init; } = string.Empty;

    public bool RequireHttpsMetadata { get; init; }
}
