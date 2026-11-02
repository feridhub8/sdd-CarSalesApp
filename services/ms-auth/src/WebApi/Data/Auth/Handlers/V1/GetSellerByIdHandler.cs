using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Queries.V1;
using WebApi.Data.Auth.Services.V1.Base;

namespace WebApi.Data.Auth.Handlers.V1;

public sealed class GetSellerByIdHandler : IRequestHandler<GetSellerByIdQuery, GetSellerOkResponseDto>
{
    private readonly IValidator<GetSellerByIdQuery> validator;
    private readonly IMapper mapper;
    private readonly IUserService userService;

    public GetSellerByIdHandler(IValidator<GetSellerByIdQuery> validator, IMapper mapper, IUserService userService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.userService = userService;
    }

    public async Task<GetSellerOkResponseDto> Handle(GetSellerByIdQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var seller = await this.userService.GetSellerAsync(request.Id, ct);

        return this.mapper.Map<GetSellerOkResponseDto>(seller);
    }
}
