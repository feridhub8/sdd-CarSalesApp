namespace WebApi.Data.Brand.Dtos.V1;

public sealed record CreateBrandDto
{
    public string Name { get; init; } = string.Empty;
}
