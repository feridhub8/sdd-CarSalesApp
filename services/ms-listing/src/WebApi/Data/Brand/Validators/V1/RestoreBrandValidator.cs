using FluentValidation;
using WebApi.Data.Brand.Commands.V1;

namespace WebApi.Data.Brand.Validators.V1;

public sealed class RestoreBrandValidator : AbstractValidator<RestoreBrandCommand>
{
    public RestoreBrandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
