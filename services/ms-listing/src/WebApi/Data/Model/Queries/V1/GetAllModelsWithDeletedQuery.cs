using MediatR;
using WebApi.Data.Model.Dtos.V1;

namespace WebApi.Data.Model.Queries.V1;

public sealed record GetAllModelsWithDeletedQuery(Guid? BrandId) : IRequest<List<GetModelWithDeletedOkResponseDto>>;
