using FluentValidation;
using WebApi.Data.Auth.Commands.V1;

namespace WebApi.Data.Auth.Validators.V1;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(command => command.Dto.Username).NotEmpty();
        RuleFor(command => command.Dto.Username)
            .Matches("^[a-zA-Z0-9._-]{3,30}$")
            .WithMessage("'{PropertyName}' must be 3-30 characters of letters, digits, '.', '_' or '-'.")
            .When(command => !string.IsNullOrEmpty(command.Dto.Username));

        RuleFor(command => command.Dto.Email).NotEmpty().EmailAddress().MaximumLength(254);

        // Unicode classes, like Keycloak's policy: 'Ə' counts as an uppercase letter, not a symbol.
        RuleFor(command => command.Dto.Password).NotEmpty();
        RuleFor(command => command.Dto.Password)
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches(@"\p{Lu}").WithMessage("'{PropertyName}' must contain an uppercase letter.")
            .Matches(@"\p{Ll}").WithMessage("'{PropertyName}' must contain a lowercase letter.")
            .Matches(@"\p{Nd}").WithMessage("'{PropertyName}' must contain a digit.")
            .Matches(@"[^\p{L}\p{Nd}]").WithMessage("'{PropertyName}' must contain a symbol.")
            .When(command => !string.IsNullOrEmpty(command.Dto.Password));

        RuleFor(command => command.Dto.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(command => command.Dto.LastName).NotEmpty().MaximumLength(100);
    }
}
