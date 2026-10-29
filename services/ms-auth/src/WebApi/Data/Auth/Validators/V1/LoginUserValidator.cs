using FluentValidation;
using WebApi.Data.Auth.Commands.V1;

namespace WebApi.Data.Auth.Validators.V1;

public sealed class LoginUserValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserValidator()
    {
        RuleFor(command => command.Dto.Login).NotEmpty();
        RuleFor(command => command.Dto.Password).NotEmpty();
    }
}
