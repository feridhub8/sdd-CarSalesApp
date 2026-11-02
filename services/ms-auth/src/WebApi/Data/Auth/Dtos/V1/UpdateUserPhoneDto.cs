namespace WebApi.Data.Auth.Dtos.V1;

public sealed record UpdateUserPhoneDto
{
    public string? Phone { get; init; }
}
