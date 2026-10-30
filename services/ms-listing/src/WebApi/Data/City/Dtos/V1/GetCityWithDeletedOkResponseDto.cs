namespace WebApi.Data.City.Dtos.V1;

public sealed record GetCityWithDeletedOkResponseDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool IsDeleted { get; init; }

    public DateTime? DeletedAt { get; init; }
}
