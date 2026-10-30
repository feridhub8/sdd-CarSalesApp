using FluentValidation;
using WebApi.Data.Listing.Commands.V1;

namespace WebApi.Data.Listing.Validators.V1;

public sealed class RestoreListingValidator : AbstractValidator<RestoreListingCommand>
{
    public RestoreListingValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
