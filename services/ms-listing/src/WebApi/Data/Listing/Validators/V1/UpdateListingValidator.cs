using FluentValidation;
using WebApi.Data.Listing.Commands.V1;
using WebApi.Data.Listing.Validators.V1.Base;

namespace WebApi.Data.Listing.Validators.V1;

public sealed class UpdateListingValidator : ListingFieldsValidatorBase<UpdateListingCommand>
{
    public UpdateListingValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
