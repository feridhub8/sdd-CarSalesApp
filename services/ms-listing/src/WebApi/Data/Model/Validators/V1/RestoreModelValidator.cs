using FluentValidation;
using WebApi.Data.Model.Commands.V1;

namespace WebApi.Data.Model.Validators.V1;

public sealed class RestoreModelValidator : AbstractValidator<RestoreModelCommand>
{
    public RestoreModelValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
