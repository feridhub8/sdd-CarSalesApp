using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.City.Dtos.V1;
using WebApi.Data.City.Queries.V1;
using WebApi.Data.City.Services.V1.Base;

namespace WebApi.Data.City.Handlers.V1;

public sealed class GetCityByIdHandler : IRequestHandler<GetCityByIdQuery, GetCityOkResponseDto>
{
    private readonly IValidator<GetCityByIdQuery> validator;
    private readonly IMapper mapper;
    private readonly ICityService cityService;

    public GetCityByIdHandler(IValidator<GetCityByIdQuery> validator, IMapper mapper, ICityService cityService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.cityService = cityService;
    }

    public async Task<GetCityOkResponseDto> Handle(GetCityByIdQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var city = await this.cityService.GetByIdAsync(request.Id, withDeleted: false, ct);

        return this.mapper.Map<GetCityOkResponseDto>(city);
    }
}
