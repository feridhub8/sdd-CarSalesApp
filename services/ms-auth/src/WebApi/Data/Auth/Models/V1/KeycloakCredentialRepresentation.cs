namespace WebApi.Data.Auth.Models.V1;

public sealed class KeycloakCredentialRepresentation
{
    public const string PasswordType = "password";

    public string Type { get; init; } = PasswordType;

    public string Value { get; init; } = string.Empty;

    public bool Temporary { get; init; }
}
