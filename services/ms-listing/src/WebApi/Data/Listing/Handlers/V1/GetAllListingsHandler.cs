using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Queries.V1;
using WebApi.Data.Listing.Services.V1.Base;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.Listing.Handlers.V1;

public sealed class GetAllListingsHandler : IRequestHandler<GetAllListingsQuery, PagedResponse<GetListingOkResponseDto>>
{
    private readonly IValidator<GetAllListingsQuery> validator;
    private readonly IMapper mapper;
    private readonly IListingService listingService;

    public GetAllListingsHandler(IValidator<GetAllListingsQuery> validator, IMapper mapper, IListingService listingService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.listingService = listingService;
    }

    public async Task<PagedResponse<GetListingOkResponseDto>> Handle(GetAllListingsQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var page = await this.listingService.GetPageAsync(request.Page, request.PageSize, withDeleted: false, ct);

        return new PagedResponse<GetListingOkResponseDto>
        {
            Items = this.mapper.Map<List<GetListingOkResponseDto>>(page.Items),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }
}
