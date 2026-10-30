using MediatR;
using WebApi.Data.Model.Dtos.V1;

namespace WebApi.Data.Model.Commands.V1;

public sealed record UpdateModelCommand(Guid Id, UpdateModelDto Dto) : IRequest<GetModelOkResponseDto>;
