namespace WebApi.Data.Auth.Dtos.V1;

// Public: only what a buyer needs. Its own type, so a new account field cannot leak here by accident.
public sealed record GetSellerOkResponseDto
{
    public string FirstName { get; init; } = string.Empty;

    public string? Phone { get; init; }
}
