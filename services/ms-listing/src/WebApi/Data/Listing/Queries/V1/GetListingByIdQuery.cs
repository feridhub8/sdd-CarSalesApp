using MediatR;
using WebApi.Data.Listing.Dtos.V1;

namespace WebApi.Data.Listing.Queries.V1;

public sealed record GetListingByIdQuery(Guid Id) : IRequest<GetListingOkResponseDto>;
