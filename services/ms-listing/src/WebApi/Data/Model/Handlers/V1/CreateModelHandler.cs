using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Model.Commands.V1;
using WebApi.Data.Model.Dtos.V1;
using WebApi.Data.Model.Services.V1.Base;

namespace WebApi.Data.Model.Handlers.V1;

public sealed class CreateModelHandler : IRequestHandler<CreateModelCommand, GetModelOkResponseDto>
{
    private readonly IValidator<CreateModelCommand> validator;
    private readonly IMapper mapper;
    private readonly IModelService modelService;

    public CreateModelHandler(IValidator<CreateModelCommand> validator, IMapper mapper, IModelService modelService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.modelService = modelService;
    }

    public async Task<GetModelOkResponseDto> Handle(CreateModelCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var model = this.mapper.Map<Entities.V1.Model>(request.Dto);
        var created = await this.modelService.CreateAsync(model, ct);

        return this.mapper.Map<GetModelOkResponseDto>(created);
    }
}
