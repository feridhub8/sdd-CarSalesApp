using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Listing.Commands.V1;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Services.V1.Base;

namespace WebApi.Data.Listing.Handlers.V1;

public sealed class UpdateListingHandler : IRequestHandler<UpdateListingCommand, GetListingOkResponseDto>
{
    private readonly IValidator<UpdateListingCommand> validator;
    private readonly IMapper mapper;
    private readonly IListingService listingService;

    public UpdateListingHandler(IValidator<UpdateListingCommand> validator, IMapper mapper, IListingService listingService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.listingService = listingService;
    }

    public async Task<GetListingOkResponseDto> Handle(UpdateListingCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        // Load first, so a missing listing (404) or someone else's listing (403) is refused before anything changes.
        var listing = await this.listingService.GetForUpdateAsync(request.Id, ct);
        this.mapper.Map(request.Dto, listing);
        var updated = await this.listingService.UpdateAsync(listing, ct);

        return this.mapper.Map<GetListingOkResponseDto>(updated);
    }
}
