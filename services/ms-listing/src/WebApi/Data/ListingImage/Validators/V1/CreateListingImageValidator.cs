using FluentValidation;
using WebApi.Data.ListingImage.Commands.V1;

namespace WebApi.Data.ListingImage.Validators.V1;

public sealed class CreateListingImageValidator : AbstractValidator<CreateListingImageCommand>
{
    public CreateListingImageValidator()
    {
        RuleFor(command => command.ListingId).NotEmpty();

        RuleFor(command => command.Dto.File)
            .NotNull()
            .WithMessage("A file is required.")
            .Must(file => file is null || file.Length > 0)
            .WithMessage("The file is empty.")
            .Must(file => file is null || file.Length <= Entities.V1.ListingImage.MaxFileBytes)
            .WithMessage($"The file must be at most {Entities.V1.ListingImage.MaxFileBytes / (1024 * 1024)} MB.");
    }
}
