using MediatR;
using WebApi.Data.City.Dtos.V1;

namespace WebApi.Data.City.Commands.V1;

public sealed record CreateCityCommand(CreateCityDto Dto) : IRequest<GetCityOkResponseDto>;
