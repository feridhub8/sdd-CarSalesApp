using FluentValidation;
using MediatR;
using WebApi.Data.Listing.Commands.V1;
using WebApi.Data.Listing.Services.V1.Base;

namespace WebApi.Data.Listing.Handlers.V1;

public sealed class DeleteListingHandler : IRequestHandler<DeleteListingCommand>
{
    private readonly IValidator<DeleteListingCommand> validator;
    private readonly IListingService listingService;

    public DeleteListingHandler(IValidator<DeleteListingCommand> validator, IListingService listingService)
    {
        this.validator = validator;
        this.listingService = listingService;
    }

    public async Task Handle(DeleteListingCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.listingService.DeleteAsync(request.Id, ct);
    }
}
