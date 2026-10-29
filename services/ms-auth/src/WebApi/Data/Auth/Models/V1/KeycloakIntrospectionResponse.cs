using System.Text.Json.Serialization;

namespace WebApi.Data.Auth.Models.V1;

public sealed class KeycloakIntrospectionResponse
{
    [JsonPropertyName("active")]
    public bool Active { get; init; }
}
