using MediatR;
using WebApi.Data.Auth.Dtos.V1;

namespace WebApi.Data.Auth.Queries.V1;

public sealed record GetSellerByIdQuery(Guid Id) : IRequest<GetSellerOkResponseDto>;
