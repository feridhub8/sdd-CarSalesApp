using MapsterMapper;
using MediatR;
using WebApi.Data.Model.Dtos.V1;
using WebApi.Data.Model.Queries.V1;
using WebApi.Data.Model.Services.V1.Base;

namespace WebApi.Data.Model.Handlers.V1;

public sealed class GetAllModelsHandler : IRequestHandler<GetAllModelsQuery, List<GetModelOkResponseDto>>
{
    private readonly IMapper mapper;
    private readonly IModelService modelService;

    public GetAllModelsHandler(IMapper mapper, IModelService modelService)
    {
        this.mapper = mapper;
        this.modelService = modelService;
    }

    public async Task<List<GetModelOkResponseDto>> Handle(GetAllModelsQuery request, CancellationToken ct)
    {
        var models = await this.modelService.GetAllAsync(request.BrandId, withDeleted: false, ct);

        return this.mapper.Map<List<GetModelOkResponseDto>>(models);
    }
}
