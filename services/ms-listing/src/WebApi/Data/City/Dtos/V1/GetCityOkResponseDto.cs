namespace WebApi.Data.City.Dtos.V1;

public sealed record GetCityOkResponseDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;
}
