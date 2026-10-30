using MediatR;
using WebApi.Data.Brand.Dtos.V1;

namespace WebApi.Data.Brand.Commands.V1;

public sealed record UpdateBrandCommand(Guid Id, UpdateBrandDto Dto) : IRequest<GetBrandOkResponseDto>;
