using FluentValidation;
using WebApi.Data.Model.Commands.V1;
using WebApi.Data.Shared.Validators.V1;

namespace WebApi.Data.Model.Validators.V1;

public sealed class CreateModelValidator : AbstractValidator<CreateModelCommand>
{
    public CreateModelValidator()
    {
        RuleFor(command => command.Dto.BrandId).NotEmpty();
        RuleFor(command => command.Dto.Name).EntryName(Entities.V1.Model.NameMaxLength);
    }
}
