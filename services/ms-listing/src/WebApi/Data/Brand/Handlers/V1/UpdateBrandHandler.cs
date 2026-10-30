using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Brand.Commands.V1;
using WebApi.Data.Brand.Dtos.V1;
using WebApi.Data.Brand.Services.V1.Base;

namespace WebApi.Data.Brand.Handlers.V1;

public sealed class UpdateBrandHandler : IRequestHandler<UpdateBrandCommand, GetBrandOkResponseDto>
{
    private readonly IValidator<UpdateBrandCommand> validator;
    private readonly IMapper mapper;
    private readonly IBrandService brandService;

    public UpdateBrandHandler(IValidator<UpdateBrandCommand> validator, IMapper mapper, IBrandService brandService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.brandService = brandService;
    }

    public async Task<GetBrandOkResponseDto> Handle(UpdateBrandCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var brand = await this.brandService.UpdateAsync(request.Id, request.Dto.Name.Trim(), ct);

        return this.mapper.Map<GetBrandOkResponseDto>(brand);
    }
}
