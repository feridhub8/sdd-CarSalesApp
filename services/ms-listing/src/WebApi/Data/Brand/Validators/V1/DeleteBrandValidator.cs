using FluentValidation;
using WebApi.Data.Brand.Commands.V1;

namespace WebApi.Data.Brand.Validators.V1;

public sealed class DeleteBrandValidator : AbstractValidator<DeleteBrandCommand>
{
    public DeleteBrandValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
