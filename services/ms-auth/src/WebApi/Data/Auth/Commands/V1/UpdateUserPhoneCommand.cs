using MediatR;
using WebApi.Data.Auth.Dtos.V1;

namespace WebApi.Data.Auth.Commands.V1;

public sealed record UpdateUserPhoneCommand(Guid UserId, UpdateUserPhoneDto Dto) : IRequest<GetUserOkResponseDto>;
