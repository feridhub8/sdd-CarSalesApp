using FluentValidation;
using WebApi.Data.ListingImage.Commands.V1;

namespace WebApi.Data.ListingImage.Validators.V1;

public sealed class DeleteListingImageValidator : AbstractValidator<DeleteListingImageCommand>
{
    public DeleteListingImageValidator()
    {
        RuleFor(command => command.Id).NotEmpty();
    }
}
