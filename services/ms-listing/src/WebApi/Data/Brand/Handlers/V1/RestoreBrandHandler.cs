using FluentValidation;
using MediatR;
using WebApi.Data.Brand.Commands.V1;
using WebApi.Data.Brand.Services.V1.Base;

namespace WebApi.Data.Brand.Handlers.V1;

public sealed class RestoreBrandHandler : IRequestHandler<RestoreBrandCommand>
{
    private readonly IValidator<RestoreBrandCommand> validator;
    private readonly IBrandService brandService;

    public RestoreBrandHandler(IValidator<RestoreBrandCommand> validator, IBrandService brandService)
    {
        this.validator = validator;
        this.brandService = brandService;
    }

    public async Task Handle(RestoreBrandCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.brandService.RestoreAsync(request.Id, ct);
    }
}
