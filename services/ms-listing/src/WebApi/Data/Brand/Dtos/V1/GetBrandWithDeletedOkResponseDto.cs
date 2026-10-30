namespace WebApi.Data.Brand.Dtos.V1;

public sealed record GetBrandWithDeletedOkResponseDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public bool IsDeleted { get; init; }

    public DateTime? DeletedAt { get; init; }
}
