namespace WebApi.Data.Brand.Dtos.V1;

public sealed record UpdateBrandDto
{
    public string Name { get; init; } = string.Empty;
}
