using FluentValidation;
using MediatR;
using WebApi.Data.City.Commands.V1;
using WebApi.Data.City.Services.V1.Base;

namespace WebApi.Data.City.Handlers.V1;

public sealed class RestoreCityHandler : IRequestHandler<RestoreCityCommand>
{
    private readonly IValidator<RestoreCityCommand> validator;
    private readonly ICityService cityService;

    public RestoreCityHandler(IValidator<RestoreCityCommand> validator, ICityService cityService)
    {
        this.validator = validator;
        this.cityService = cityService;
    }

    public async Task Handle(RestoreCityCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.cityService.RestoreAsync(request.Id, ct);
    }
}
