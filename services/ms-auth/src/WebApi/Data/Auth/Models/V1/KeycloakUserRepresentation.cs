using System.Text.Json.Serialization;

namespace WebApi.Data.Auth.Models.V1;

public sealed class KeycloakUserRepresentation
{
    public const string PhoneAttribute = "phone";

    public string? Id { get; set; }

    public string? Username { get; init; }

    public string? Email { get; init; }

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public bool Enabled { get; init; }

    public List<KeycloakCredentialRepresentation>? Credentials { get; init; }

    public Dictionary<string, List<string>>? Attributes { get; init; }

    // Read side only; a phone is written through IUserRepository.UpdatePhoneAsync.
    [JsonIgnore]
    public string? Phone => this.Attributes?.GetValueOrDefault(PhoneAttribute)?.FirstOrDefault();
}
