using FluentValidation;
using WebApi.Data.Auth.Commands.V1;

namespace WebApi.Data.Auth.Validators.V1;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(command => command.Dto.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(command => command.Dto.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(command => command.Dto.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Dto.LastName).NotEmpty().MaximumLength(100);
    }
}
