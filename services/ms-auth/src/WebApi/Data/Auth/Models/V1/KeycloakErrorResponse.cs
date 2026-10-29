using System.Text.Json.Serialization;

namespace WebApi.Data.Auth.Models.V1;

public sealed class KeycloakErrorResponse
{
    public const string InvalidGrant = "invalid_grant";
    public const string InvalidPasswordPrefix = "invalidPassword";

    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; init; }

    // The admin API reports conflicts as {"errorMessage": "..."}.
    [JsonPropertyName("errorMessage")]
    public string? ErrorMessage { get; init; }
}
