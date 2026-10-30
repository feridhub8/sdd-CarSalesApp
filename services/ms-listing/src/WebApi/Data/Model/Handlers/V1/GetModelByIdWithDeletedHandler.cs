using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Model.Dtos.V1;
using WebApi.Data.Model.Queries.V1;
using WebApi.Data.Model.Services.V1.Base;

namespace WebApi.Data.Model.Handlers.V1;

public sealed class GetModelByIdWithDeletedHandler : IRequestHandler<GetModelByIdWithDeletedQuery, GetModelWithDeletedOkResponseDto>
{
    private readonly IValidator<GetModelByIdWithDeletedQuery> validator;
    private readonly IMapper mapper;
    private readonly IModelService modelService;

    public GetModelByIdWithDeletedHandler(IValidator<GetModelByIdWithDeletedQuery> validator, IMapper mapper, IModelService modelService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.modelService = modelService;
    }

    public async Task<GetModelWithDeletedOkResponseDto> Handle(GetModelByIdWithDeletedQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var model = await this.modelService.GetByIdAsync(request.Id, withDeleted: true, ct);

        return this.mapper.Map<GetModelWithDeletedOkResponseDto>(model);
    }
}
