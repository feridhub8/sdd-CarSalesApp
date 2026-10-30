namespace WebApi.Data.City.Dtos.V1;

public sealed record CreateCityDto
{
    public string Name { get; init; } = string.Empty;
}
