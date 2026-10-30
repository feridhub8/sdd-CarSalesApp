using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.City.Commands.V1;
using WebApi.Data.City.Dtos.V1;
using WebApi.Data.City.Services.V1.Base;

namespace WebApi.Data.City.Handlers.V1;

public sealed class UpdateCityHandler : IRequestHandler<UpdateCityCommand, GetCityOkResponseDto>
{
    private readonly IValidator<UpdateCityCommand> validator;
    private readonly IMapper mapper;
    private readonly ICityService cityService;

    public UpdateCityHandler(IValidator<UpdateCityCommand> validator, IMapper mapper, ICityService cityService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.cityService = cityService;
    }

    public async Task<GetCityOkResponseDto> Handle(UpdateCityCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var city = await this.cityService.UpdateAsync(request.Id, request.Dto.Name.Trim(), ct);

        return this.mapper.Map<GetCityOkResponseDto>(city);
    }
}
