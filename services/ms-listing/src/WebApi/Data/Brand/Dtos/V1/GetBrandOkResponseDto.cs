namespace WebApi.Data.Brand.Dtos.V1;

public sealed record GetBrandOkResponseDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;
}
