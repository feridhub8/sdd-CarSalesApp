using FluentValidation;
using MediatR;
using WebApi.Data.Brand.Commands.V1;
using WebApi.Data.Brand.Services.V1.Base;

namespace WebApi.Data.Brand.Handlers.V1;

public sealed class DeleteBrandHandler : IRequestHandler<DeleteBrandCommand>
{
    private readonly IValidator<DeleteBrandCommand> validator;
    private readonly IBrandService brandService;

    public DeleteBrandHandler(IValidator<DeleteBrandCommand> validator, IBrandService brandService)
    {
        this.validator = validator;
        this.brandService = brandService;
    }

    public async Task Handle(DeleteBrandCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        await this.brandService.DeleteAsync(request.Id, ct);
    }
}
