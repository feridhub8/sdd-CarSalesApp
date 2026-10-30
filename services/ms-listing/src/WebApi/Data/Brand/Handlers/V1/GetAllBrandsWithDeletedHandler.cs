using MapsterMapper;
using MediatR;
using WebApi.Data.Brand.Dtos.V1;
using WebApi.Data.Brand.Queries.V1;
using WebApi.Data.Brand.Services.V1.Base;

namespace WebApi.Data.Brand.Handlers.V1;

public sealed class GetAllBrandsWithDeletedHandler : IRequestHandler<GetAllBrandsWithDeletedQuery, List<GetBrandWithDeletedOkResponseDto>>
{
    private readonly IMapper mapper;
    private readonly IBrandService brandService;

    public GetAllBrandsWithDeletedHandler(IMapper mapper, IBrandService brandService)
    {
        this.mapper = mapper;
        this.brandService = brandService;
    }

    public async Task<List<GetBrandWithDeletedOkResponseDto>> Handle(GetAllBrandsWithDeletedQuery request, CancellationToken ct)
    {
        var brands = await this.brandService.GetAllAsync(withDeleted: true, ct);

        return this.mapper.Map<List<GetBrandWithDeletedOkResponseDto>>(brands);
    }
}
