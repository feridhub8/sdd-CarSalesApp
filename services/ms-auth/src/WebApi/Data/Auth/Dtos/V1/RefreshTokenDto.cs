namespace WebApi.Data.Auth.Dtos.V1;

public sealed record RefreshTokenDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
