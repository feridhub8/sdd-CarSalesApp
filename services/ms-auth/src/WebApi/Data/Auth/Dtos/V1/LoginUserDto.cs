namespace WebApi.Data.Auth.Dtos.V1;

public sealed record LoginUserDto
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}
