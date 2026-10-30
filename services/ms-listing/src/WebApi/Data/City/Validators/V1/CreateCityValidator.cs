using FluentValidation;
using WebApi.Data.City.Commands.V1;
using WebApi.Data.Shared.Validators.V1;

namespace WebApi.Data.City.Validators.V1;

public sealed class CreateCityValidator : AbstractValidator<CreateCityCommand>
{
    public CreateCityValidator()
    {
        RuleFor(command => command.Dto.Name).EntryName(Entities.V1.City.NameMaxLength);
    }
}
