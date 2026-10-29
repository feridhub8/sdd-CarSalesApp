using FluentValidation;
using WebApi.Data.Auth.Commands.V1;

namespace WebApi.Data.Auth.Validators.V1;

public sealed class LogoutUserValidator : AbstractValidator<LogoutUserCommand>
{
    public LogoutUserValidator()
    {
        RuleFor(command => command.Dto.RefreshToken).NotEmpty();
    }
}
