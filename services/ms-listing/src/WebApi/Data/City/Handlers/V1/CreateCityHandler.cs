using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.City.Commands.V1;
using WebApi.Data.City.Dtos.V1;
using WebApi.Data.City.Services.V1.Base;

namespace WebApi.Data.City.Handlers.V1;

public sealed class CreateCityHandler : IRequestHandler<CreateCityCommand, GetCityOkResponseDto>
{
    private readonly IValidator<CreateCityCommand> validator;
    private readonly IMapper mapper;
    private readonly ICityService cityService;

    public CreateCityHandler(IValidator<CreateCityCommand> validator, IMapper mapper, ICityService cityService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.cityService = cityService;
    }

    public async Task<GetCityOkResponseDto> Handle(CreateCityCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var city = this.mapper.Map<Entities.V1.City>(request.Dto);
        var created = await this.cityService.CreateAsync(city, ct);

        return this.mapper.Map<GetCityOkResponseDto>(created);
    }
}
