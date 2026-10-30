namespace WebApi.Data.Model.Dtos.V1;

public sealed record GetModelOkResponseDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public Guid BrandId { get; init; }

    public string BrandName { get; init; } = string.Empty;
}
