using MapsterMapper;
using MediatR;
using WebApi.Data.City.Dtos.V1;
using WebApi.Data.City.Queries.V1;
using WebApi.Data.City.Services.V1.Base;

namespace WebApi.Data.City.Handlers.V1;

public sealed class GetAllCitiesWithDeletedHandler : IRequestHandler<GetAllCitiesWithDeletedQuery, List<GetCityWithDeletedOkResponseDto>>
{
    private readonly IMapper mapper;
    private readonly ICityService cityService;

    public GetAllCitiesWithDeletedHandler(IMapper mapper, ICityService cityService)
    {
        this.mapper = mapper;
        this.cityService = cityService;
    }

    public async Task<List<GetCityWithDeletedOkResponseDto>> Handle(GetAllCitiesWithDeletedQuery request, CancellationToken ct)
    {
        var cities = await this.cityService.GetAllAsync(withDeleted: true, ct);

        return this.mapper.Map<List<GetCityWithDeletedOkResponseDto>>(cities);
    }
}
