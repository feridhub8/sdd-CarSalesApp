using System.Text.Json.Serialization;

namespace WebApi.Data.Auth.Dtos.V1;

public sealed record GetTokenOkResponseDto
{
    public string AccessToken { get; init; } = string.Empty;

    // Web clients get the refresh token as a cookie instead, so it is left out of the body.
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RefreshToken { get; init; }

    public int ExpiresIn { get; init; }

    public int RefreshExpiresIn { get; init; }

    public string TokenType { get; init; } = string.Empty;
}
