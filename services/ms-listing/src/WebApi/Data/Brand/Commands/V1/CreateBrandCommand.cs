using MediatR;
using WebApi.Data.Brand.Dtos.V1;

namespace WebApi.Data.Brand.Commands.V1;

public sealed record CreateBrandCommand(CreateBrandDto Dto) : IRequest<GetBrandOkResponseDto>;
