using System.Text.Json.Serialization;

namespace WebApi.Data.Shared.Models.V1;

public sealed class KeycloakIntrospectionResponse
{
    [JsonPropertyName("active")]
    public bool Active { get; init; }
}
