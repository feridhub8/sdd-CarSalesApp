using FluentValidation;
using WebApi.Data.Brand.Commands.V1;
using WebApi.Data.Shared.Validators.V1;

namespace WebApi.Data.Brand.Validators.V1;

public sealed class CreateBrandValidator : AbstractValidator<CreateBrandCommand>
{
    public CreateBrandValidator()
    {
        RuleFor(command => command.Dto.Name).EntryName(Entities.V1.Brand.NameMaxLength);
    }
}
