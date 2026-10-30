using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Queries.V1;
using WebApi.Data.Listing.Services.V1.Base;

namespace WebApi.Data.Listing.Handlers.V1;

public sealed class GetListingByIdHandler : IRequestHandler<GetListingByIdQuery, GetListingOkResponseDto>
{
    private readonly IValidator<GetListingByIdQuery> validator;
    private readonly IMapper mapper;
    private readonly IListingService listingService;

    public GetListingByIdHandler(IValidator<GetListingByIdQuery> validator, IMapper mapper, IListingService listingService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.listingService = listingService;
    }

    public async Task<GetListingOkResponseDto> Handle(GetListingByIdQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var listing = await this.listingService.GetByIdAsync(request.Id, withDeleted: false, ct);

        return this.mapper.Map<GetListingOkResponseDto>(listing);
    }
}
