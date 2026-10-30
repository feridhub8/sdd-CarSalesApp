using MapsterMapper;
using MediatR;
using WebApi.Data.City.Dtos.V1;
using WebApi.Data.City.Queries.V1;
using WebApi.Data.City.Services.V1.Base;

namespace WebApi.Data.City.Handlers.V1;

public sealed class GetAllCitiesHandler : IRequestHandler<GetAllCitiesQuery, List<GetCityOkResponseDto>>
{
    private readonly IMapper mapper;
    private readonly ICityService cityService;

    public GetAllCitiesHandler(IMapper mapper, ICityService cityService)
    {
        this.mapper = mapper;
        this.cityService = cityService;
    }

    public async Task<List<GetCityOkResponseDto>> Handle(GetAllCitiesQuery request, CancellationToken ct)
    {
        var cities = await this.cityService.GetAllAsync(withDeleted: false, ct);

        return this.mapper.Map<List<GetCityOkResponseDto>>(cities);
    }
}
