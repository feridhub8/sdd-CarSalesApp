using MediatR;
using WebApi.Data.Auth.Dtos.V1;

namespace WebApi.Data.Auth.Commands.V1;

public sealed record LoginUserCommand(LoginUserDto Dto) : IRequest<GetTokenOkResponseDto>;
