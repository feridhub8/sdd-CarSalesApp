using MediatR;
using WebApi.Data.Listing.Dtos.V1;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.Listing.Queries.V1;

public sealed record GetAllListingsQuery(int Page, int PageSize) : IRequest<PagedResponse<GetListingOkResponseDto>>;
