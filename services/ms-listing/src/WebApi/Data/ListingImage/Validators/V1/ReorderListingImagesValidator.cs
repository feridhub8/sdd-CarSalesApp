using FluentValidation;
using WebApi.Data.ListingImage.Commands.V1;

namespace WebApi.Data.ListingImage.Validators.V1;

public sealed class ReorderListingImagesValidator : AbstractValidator<ReorderListingImagesCommand>
{
    public ReorderListingImagesValidator()
    {
        RuleFor(command => command.ListingId).NotEmpty();

        RuleFor(command => command.Dto.ImageIds)
            .NotNull()
            .WithMessage("The list of image ids is required.");
    }
}
