using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Listing.Queries.V1;
using WebApi.Data.Listing.Services.V1.Base;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.Listing.Handlers.V1;

public sealed class GetAllListingsWithDeletedHandler : IRequestHandler<GetAllListingsWithDeletedQuery, PagedResponse<GetListingWithDeletedOkResponseDto>>
{
    private readonly IValidator<GetAllListingsWithDeletedQuery> validator;
    private readonly IMapper mapper;
    private readonly IListingService listingService;

    public GetAllListingsWithDeletedHandler(IValidator<GetAllListingsWithDeletedQuery> validator, IMapper mapper, IListingService listingService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.listingService = listingService;
    }

    public async Task<PagedResponse<GetListingWithDeletedOkResponseDto>> Handle(GetAllListingsWithDeletedQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var page = await this.listingService.GetPageAsync(request.Page, request.PageSize, withDeleted: true, ct);

        return new PagedResponse<GetListingWithDeletedOkResponseDto>
        {
            Items = this.mapper.Map<List<GetListingWithDeletedOkResponseDto>>(page.Items),
            Page = page.Page,
            PageSize = page.PageSize,
            TotalCount = page.TotalCount
        };
    }
}
