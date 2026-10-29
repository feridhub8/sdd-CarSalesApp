namespace WebApi.Data.Auth.Models.V1;

public sealed class KeycloakUserRepresentation
{
    public string? Id { get; set; }

    public string? Username { get; init; }

    public string? Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public bool Enabled { get; init; }

    public List<KeycloakCredentialRepresentation>? Credentials { get; init; }
}
