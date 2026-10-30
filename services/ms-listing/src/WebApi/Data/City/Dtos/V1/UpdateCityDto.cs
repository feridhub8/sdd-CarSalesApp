namespace WebApi.Data.City.Dtos.V1;

public sealed record UpdateCityDto
{
    public string Name { get; init; } = string.Empty;
}
