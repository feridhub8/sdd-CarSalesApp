using FluentValidation;
using MediatR;
using WebApi.Data.City.Commands.V1;
using WebApi.Data.City.Services.V1.Base;

namespace WebApi.Data.City.Handlers.V1;

public sealed class DeleteCityHandler : IRequestHandler<DeleteCityCommand>
{
    private readonly IValidator<DeleteCityCommand> validator;
    private readonly ICityService cityService;

    public DeleteCityHandler(IValidator<DeleteCityCommand> validator, ICityService cityService)
    {
        this.validator = validator;
        this.cityService = cityService;
    }

    public async Task Handle(DeleteCityCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.cityService.DeleteAsync(request.Id, ct);
    }
}
