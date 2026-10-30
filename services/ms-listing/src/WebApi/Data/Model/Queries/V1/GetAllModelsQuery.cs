using MediatR;
using WebApi.Data.Model.Dtos.V1;

namespace WebApi.Data.Model.Queries.V1;

public sealed record GetAllModelsQuery(Guid? BrandId) : IRequest<List<GetModelOkResponseDto>>;
