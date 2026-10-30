namespace WebApi.Data.Model.Dtos.V1;

public sealed record CreateModelDto
{
    public Guid BrandId { get; init; }

    public string Name { get; init; } = string.Empty;
}
