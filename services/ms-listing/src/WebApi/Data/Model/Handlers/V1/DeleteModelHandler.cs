using FluentValidation;
using MediatR;
using WebApi.Data.Model.Commands.V1;
using WebApi.Data.Model.Services.V1.Base;

namespace WebApi.Data.Model.Handlers.V1;

public sealed class DeleteModelHandler : IRequestHandler<DeleteModelCommand>
{
    private readonly IValidator<DeleteModelCommand> validator;
    private readonly IModelService modelService;

    public DeleteModelHandler(IValidator<DeleteModelCommand> validator, IModelService modelService)
    {
        this.validator = validator;
        this.modelService = modelService;
    }

    public async Task Handle(DeleteModelCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.modelService.DeleteAsync(request.Id, ct);
    }
}
