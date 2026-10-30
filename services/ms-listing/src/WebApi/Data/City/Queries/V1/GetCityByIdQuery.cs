using MediatR;
using WebApi.Data.City.Dtos.V1;

namespace WebApi.Data.City.Queries.V1;

public sealed record GetCityByIdQuery(Guid Id) : IRequest<GetCityOkResponseDto>;
