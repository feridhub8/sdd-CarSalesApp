using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Queries.V1;
using WebApi.Data.Listing.Services.V1.Base;

namespace WebApi.Data.Listing.Handlers.V1;

public sealed class GetListingByIdWithDeletedHandler : IRequestHandler<GetListingByIdWithDeletedQuery, GetListingWithDeletedOkResponseDto>
{
    private readonly IValidator<GetListingByIdWithDeletedQuery> validator;
    private readonly IMapper mapper;
    private readonly IListingService listingService;

    public GetListingByIdWithDeletedHandler(IValidator<GetListingByIdWithDeletedQuery> validator, IMapper mapper, IListingService listingService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.listingService = listingService;
    }

    public async Task<GetListingWithDeletedOkResponseDto> Handle(GetListingByIdWithDeletedQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var listing = await this.listingService.GetByIdAsync(request.Id, withDeleted: true, ct);

        return this.mapper.Map<GetListingWithDeletedOkResponseDto>(listing);
    }
}
