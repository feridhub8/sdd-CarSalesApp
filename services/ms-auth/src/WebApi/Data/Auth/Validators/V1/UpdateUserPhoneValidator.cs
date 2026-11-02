using FluentValidation;
using WebApi.Data.Auth.Commands.V1;

namespace WebApi.Data.Auth.Validators.V1;

public sealed class UpdateUserPhoneValidator : AbstractValidator<UpdateUserPhoneCommand>
{
    public UpdateUserPhoneValidator()
    {
        RuleFor(command => command.UserId).NotEmpty();

        RuleFor(command => command.Dto.Phone)
            .Must(phone => phone.NormalizePhone() is not null)
            .WithMessage(PhoneRuleExtensions.InvalidMessage);
    }
}
