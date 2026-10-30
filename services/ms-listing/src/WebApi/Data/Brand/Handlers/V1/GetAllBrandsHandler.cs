using MapsterMapper;
using MediatR;
using WebApi.Data.Brand.Dtos.V1;
using WebApi.Data.Brand.Queries.V1;
using WebApi.Data.Brand.Services.V1.Base;

namespace WebApi.Data.Brand.Handlers.V1;

public sealed class GetAllBrandsHandler : IRequestHandler<GetAllBrandsQuery, List<GetBrandOkResponseDto>>
{
    private readonly IMapper mapper;
    private readonly IBrandService brandService;

    public GetAllBrandsHandler(IMapper mapper, IBrandService brandService)
    {
        this.mapper = mapper;
        this.brandService = brandService;
    }

    public async Task<List<GetBrandOkResponseDto>> Handle(GetAllBrandsQuery request, CancellationToken ct)
    {
        var brands = await this.brandService.GetAllAsync(withDeleted: false, ct);

        return this.mapper.Map<List<GetBrandOkResponseDto>>(brands);
    }
}
