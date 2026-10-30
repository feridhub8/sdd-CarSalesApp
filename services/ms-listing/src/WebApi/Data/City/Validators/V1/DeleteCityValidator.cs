using FluentValidation;
using WebApi.Data.City.Commands.V1;

namespace WebApi.Data.City.Validators.V1;

public sealed class DeleteCityValidator : AbstractValidator<DeleteCityCommand>
{
    public DeleteCityValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
