using FluentValidation;
using WebApi.Data.City.Commands.V1;
using WebApi.Data.Shared.Validators.V1;

namespace WebApi.Data.City.Validators.V1;

public sealed class UpdateCityValidator : AbstractValidator<UpdateCityCommand>
{
    public UpdateCityValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
        RuleFor(command => command.Dto.Name).EntryName(Entities.V1.City.NameMaxLength);
    }
}
