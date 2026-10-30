using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Model.Dtos.V1;
using WebApi.Data.Model.Queries.V1;
using WebApi.Data.Model.Services.V1.Base;

namespace WebApi.Data.Model.Handlers.V1;

public sealed class GetModelByIdHandler : IRequestHandler<GetModelByIdQuery, GetModelOkResponseDto>
{
    private readonly IValidator<GetModelByIdQuery> validator;
    private readonly IMapper mapper;
    private readonly IModelService modelService;

    public GetModelByIdHandler(IValidator<GetModelByIdQuery> validator, IMapper mapper, IModelService modelService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.modelService = modelService;
    }

    public async Task<GetModelOkResponseDto> Handle(GetModelByIdQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var model = await this.modelService.GetByIdAsync(request.Id, withDeleted: false, ct);

        return this.mapper.Map<GetModelOkResponseDto>(model);
    }
}
