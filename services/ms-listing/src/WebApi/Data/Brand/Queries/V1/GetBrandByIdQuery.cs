using MediatR;
using WebApi.Data.Brand.Dtos.V1;

namespace WebApi.Data.Brand.Queries.V1;

public sealed record GetBrandByIdQuery(Guid Id) : IRequest<GetBrandOkResponseDto>;
