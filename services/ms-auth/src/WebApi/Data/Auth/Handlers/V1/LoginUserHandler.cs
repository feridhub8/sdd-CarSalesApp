using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Auth.Commands.V1;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Services.V1.Base;

namespace WebApi.Data.Auth.Handlers.V1;

public sealed class LoginUserHandler : IRequestHandler<LoginUserCommand, GetTokenOkResponseDto>
{
    private readonly IValidator<LoginUserCommand> validator;
    private readonly IMapper mapper;
    private readonly ITokenService tokenService;

    public LoginUserHandler(IValidator<LoginUserCommand> validator, IMapper mapper, ITokenService tokenService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.tokenService = tokenService;
    }

    public async Task<GetTokenOkResponseDto> Handle(LoginUserCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var token = await this.tokenService.LoginAsync(request.Dto.Login, request.Dto.Password, ct);

        return this.mapper.Map<GetTokenOkResponseDto>(token);
    }
}
