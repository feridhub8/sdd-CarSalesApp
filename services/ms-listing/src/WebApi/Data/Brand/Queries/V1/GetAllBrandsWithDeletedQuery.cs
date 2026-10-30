using MediatR;
using WebApi.Data.Brand.Dtos.V1;

namespace WebApi.Data.Brand.Queries.V1;

public sealed record GetAllBrandsWithDeletedQuery : IRequest<List<GetBrandWithDeletedOkResponseDto>>;
