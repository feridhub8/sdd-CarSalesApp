using FluentValidation;
using WebApi.Data.Listing.Commands.V1;

namespace WebApi.Data.Listing.Validators.V1;

public sealed class DeleteListingValidator : AbstractValidator<DeleteListingCommand>
{
    public DeleteListingValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
