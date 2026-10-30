using FluentValidation;
using WebApi.Data.Model.Commands.V1;

namespace WebApi.Data.Model.Validators.V1;

public sealed class DeleteModelValidator : AbstractValidator<DeleteModelCommand>
{
    public DeleteModelValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
