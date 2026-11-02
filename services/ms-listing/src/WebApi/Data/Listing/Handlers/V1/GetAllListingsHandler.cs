using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Models.V1;
using WebApi.Data.Listing.Queries.V1;
using WebApi.Data.Listing.Services.V1.Base;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.Listing.Handlers.V1;

public sealed class GetAllListingsHandler : IRequestHandler<GetAllListingsQuery, PagedResponse<GetListingOkResponseDto>>
{
    private readonly IValidator<SearchListingsDto> validator;
    private readonly IMapper mapper;
    private readonly IListingService listingService;

    public GetAllListingsHandler(IValidator<SearchListingsDto> validator, IMapper mapper, IListingService listingService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.listingService = listingService;
    }

    public async Task<PagedResponse<GetListingOkResponseDto>> Handle(GetAllListingsQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request.Search, ct);

        var page = await this.listingService.SearchAsync(this.mapper.Map<ListingSearch>(request.Search), ct);

        return new PagedResponse<GetListingOkResponseDto>
        {
            // A list shows only each listing's cover.
            Items = this.mapper.Map<List<GetListingOkResponseDto>>(page.Items).Select(item => item with { Images = null }).ToList(),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }
}
