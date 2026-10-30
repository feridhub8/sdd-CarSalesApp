using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Brand.Dtos.V1;
using WebApi.Data.Brand.Queries.V1;
using WebApi.Data.Brand.Services.V1.Base;

namespace WebApi.Data.Brand.Handlers.V1;

public sealed class GetBrandByIdWithDeletedHandler : IRequestHandler<GetBrandByIdWithDeletedQuery, GetBrandWithDeletedOkResponseDto>
{
    private readonly IValidator<GetBrandByIdWithDeletedQuery> validator;
    private readonly IMapper mapper;
    private readonly IBrandService brandService;

    public GetBrandByIdWithDeletedHandler(IValidator<GetBrandByIdWithDeletedQuery> validator, IMapper mapper, IBrandService brandService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.brandService = brandService;
    }

    public async Task<GetBrandWithDeletedOkResponseDto> Handle(GetBrandByIdWithDeletedQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var brand = await this.brandService.GetByIdAsync(request.Id, withDeleted: true, ct);

        return this.mapper.Map<GetBrandWithDeletedOkResponseDto>(brand);
    }
}
