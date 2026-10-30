using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Listing.Commands.V1;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Services.V1.Base;

namespace WebApi.Data.Listing.Handlers.V1;

public sealed class CreateListingHandler : IRequestHandler<CreateListingCommand, GetListingOkResponseDto>
{
    private readonly IValidator<CreateListingCommand> validator;
    private readonly IMapper mapper;
    private readonly IListingService listingService;

    public CreateListingHandler(IValidator<CreateListingCommand> validator, IMapper mapper, IListingService listingService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.listingService = listingService;
    }

    public async Task<GetListingOkResponseDto> Handle(CreateListingCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var listing = this.mapper.Map<Entities.V1.Listing>(request.Dto);
        var created = await this.listingService.CreateAsync(listing, ct);

        return this.mapper.Map<GetListingOkResponseDto>(created);
    }
}
