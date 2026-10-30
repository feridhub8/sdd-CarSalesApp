using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Model.Commands.V1;
using WebApi.Data.Model.Dtos.V1;
using WebApi.Data.Model.Services.V1.Base;

namespace WebApi.Data.Model.Handlers.V1;

public sealed class UpdateModelHandler : IRequestHandler<UpdateModelCommand, GetModelOkResponseDto>
{
    private readonly IValidator<UpdateModelCommand> validator;
    private readonly IMapper mapper;
    private readonly IModelService modelService;

    public UpdateModelHandler(IValidator<UpdateModelCommand> validator, IMapper mapper, IModelService modelService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.modelService = modelService;
    }

    public async Task<GetModelOkResponseDto> Handle(UpdateModelCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var model = await this.modelService.UpdateAsync(request.Id, request.Dto.BrandId, request.Dto.Name.Trim(), ct);

        return this.mapper.Map<GetModelOkResponseDto>(model);
    }
}
