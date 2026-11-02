using FluentValidation;
using MediatR;
using WebApi.Data.ListingImage.Commands.V1;
using WebApi.Data.ListingImage.Services.V1.Base;

namespace WebApi.Data.ListingImage.Handlers.V1;

public sealed class DeleteListingImageHandler : IRequestHandler<DeleteListingImageCommand>
{
    private readonly IValidator<DeleteListingImageCommand> validator;
    private readonly IListingImageService listingImageService;

    public DeleteListingImageHandler(IValidator<DeleteListingImageCommand> validator, IListingImageService listingImageService)
    {
        this.validator = validator;
        this.listingImageService = listingImageService;
    }

    public async Task Handle(DeleteListingImageCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.listingImageService.DeleteAsync(request.Id, ct);
    }
}
