using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Queries.V1;
using WebApi.Data.Auth.Services.V1.Base;

namespace WebApi.Data.Auth.Handlers.V1;

public sealed class GetUserByIdHandler : IRequestHandler<GetUserByIdQuery, GetUserOkResponseDto>
{
    private readonly IValidator<GetUserByIdQuery> validator;
    private readonly IMapper mapper;
    private readonly IUserService userService;

    public GetUserByIdHandler(IValidator<GetUserByIdQuery> validator, IMapper mapper, IUserService userService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.userService = userService;
    }

    public async Task<GetUserOkResponseDto> Handle(GetUserByIdQuery request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var user = await this.userService.GetByIdAsync(request.Id, ct);

        return this.mapper.Map<GetUserOkResponseDto>(user);
    }
}
