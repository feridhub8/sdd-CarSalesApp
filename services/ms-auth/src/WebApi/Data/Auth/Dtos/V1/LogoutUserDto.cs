namespace WebApi.Data.Auth.Dtos.V1;

public sealed record LogoutUserDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
