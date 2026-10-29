namespace WebApi.Data.Auth.Dtos.V1;

public sealed record GetTokenOkResponseDto
{
    public string AccessToken { get; init; } = string.Empty;

    public string RefreshToken { get; init; } = string.Empty;

    public int ExpiresIn { get; init; }

    public int RefreshExpiresIn { get; init; }

    public string TokenType { get; init; } = string.Empty;
}
