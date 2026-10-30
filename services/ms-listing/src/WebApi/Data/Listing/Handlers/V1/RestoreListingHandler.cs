using FluentValidation;
using MediatR;
using WebApi.Data.Listing.Commands.V1;
using WebApi.Data.Listing.Services.V1.Base;

namespace WebApi.Data.Listing.Handlers.V1;

public sealed class RestoreListingHandler : IRequestHandler<RestoreListingCommand>
{
    private readonly IValidator<RestoreListingCommand> validator;
    private readonly IListingService listingService;

    public RestoreListingHandler(IValidator<RestoreListingCommand> validator, IListingService listingService)
    {
        this.validator = validator;
        this.listingService = listingService;
    }

    public async Task Handle(RestoreListingCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.listingService.RestoreAsync(request.Id, ct);
    }
}
