using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.City.Dtos.V1;
using WebApi.Data.City.Queries.V1;
using WebApi.Data.City.Services.V1.Base;

namespace WebApi.Data.City.Handlers.V1;

public sealed class GetCityByIdWithDeletedHandler : IRequestHandler<GetCityByIdWithDeletedQuery, GetCityWithDeletedOkResponseDto>
{
    private readonly IValidator<GetCityByIdWithDeletedQuery> validator;
    private readonly IMapper mapper;
    private readonly ICityService cityService;

    public GetCityByIdWithDeletedHandler(IValidator<GetCityByIdWithDeletedQuery> validator, IMapper mapper, ICityService cityService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.cityService = cityService;
    }

    public async Task<GetCityWithDeletedOkResponseDto> Handle(GetCityByIdWithDeletedQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var city = await this.cityService.GetByIdAsync(request.Id, withDeleted: true, ct);

        return this.mapper.Map<GetCityWithDeletedOkResponseDto>(city);
    }
}
