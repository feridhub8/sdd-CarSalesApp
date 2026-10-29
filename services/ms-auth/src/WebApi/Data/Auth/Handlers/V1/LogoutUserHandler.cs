using FluentValidation;
using MediatR;
using WebApi.Data.Auth.Commands.V1;
using WebApi.Data.Auth.Services.V1.Base;

namespace WebApi.Data.Auth.Handlers.V1;

public sealed class LogoutUserHandler : IRequestHandler<LogoutUserCommand>
{
    private readonly IValidator<LogoutUserCommand> validator;
    private readonly ITokenService tokenService;

    public LogoutUserHandler(IValidator<LogoutUserCommand> validator, ITokenService tokenService)
    {
        this.validator = validator;
        this.tokenService = tokenService;
    }

    public async Task Handle(LogoutUserCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.tokenService.LogoutAsync(request.Dto.RefreshToken, ct);
    }
}
