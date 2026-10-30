namespace WebApi.Data.Model.Dtos.V1;

public sealed record UpdateModelDto
{
    public Guid BrandId { get; init; }

    public string Name { get; init; } = string.Empty;
}
