using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Brand.Commands.V1;
using WebApi.Data.Brand.Dtos.V1;
using WebApi.Data.Brand.Services.V1.Base;

namespace WebApi.Data.Brand.Handlers.V1;

public sealed class CreateBrandHandler : IRequestHandler<CreateBrandCommand, GetBrandOkResponseDto>
{
    private readonly IValidator<CreateBrandCommand> validator;
    private readonly IMapper mapper;
    private readonly IBrandService brandService;

    public CreateBrandHandler(IValidator<CreateBrandCommand> validator, IMapper mapper, IBrandService brandService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.brandService = brandService;
    }

    public async Task<GetBrandOkResponseDto> Handle(CreateBrandCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var brand = this.mapper.Map<Entities.V1.Brand>(request.Dto);
        var created = await this.brandService.CreateAsync(brand, ct);

        return this.mapper.Map<GetBrandOkResponseDto>(created);
    }
}
