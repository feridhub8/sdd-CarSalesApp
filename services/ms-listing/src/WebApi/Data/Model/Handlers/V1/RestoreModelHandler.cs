using FluentValidation;
using MediatR;
using WebApi.Data.Model.Commands.V1;
using WebApi.Data.Model.Services.V1.Base;

namespace WebApi.Data.Model.Handlers.V1;

public sealed class RestoreModelHandler : IRequestHandler<RestoreModelCommand>
{
    private readonly IValidator<RestoreModelCommand> validator;
    private readonly IModelService modelService;

    public RestoreModelHandler(IValidator<RestoreModelCommand> validator, IModelService modelService)
    {
        this.validator = validator;
        this.modelService = modelService;
    }

    public async Task Handle(RestoreModelCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.modelService.RestoreAsync(request.Id, ct);
    }
}
