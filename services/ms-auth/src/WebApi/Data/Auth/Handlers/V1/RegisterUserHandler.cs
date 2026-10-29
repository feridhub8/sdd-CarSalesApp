using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Auth.Commands.V1;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Models.V1;
using WebApi.Data.Auth.Services.V1.Base;

namespace WebApi.Data.Auth.Handlers.V1;

public sealed class RegisterUserHandler : IRequestHandler<RegisterUserCommand, GetUserOkResponseDto>
{
    private readonly IValidator<RegisterUserCommand> validator;
    private readonly IMapper mapper;
    private readonly IUserService userService;

    public RegisterUserHandler(IValidator<RegisterUserCommand> validator, IMapper mapper, IUserService userService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.userService = userService;
    }

    public async Task<GetUserOkResponseDto> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var user = this.mapper.Map<KeycloakUserRepresentation>(request.Dto);
        var created = await this.userService.RegisterAsync(user, ct);

        return this.mapper.Map<GetUserOkResponseDto>(created);
    }
}
