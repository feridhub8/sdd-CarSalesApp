using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Auth.Commands.V1;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Services.V1.Base;

namespace WebApi.Data.Auth.Handlers.V1;

public sealed class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, GetTokenOkResponseDto>
{
    private readonly IValidator<RefreshTokenCommand> validator;
    private readonly IMapper mapper;
    private readonly ITokenService tokenService;

    public RefreshTokenHandler(IValidator<RefreshTokenCommand> validator, IMapper mapper, ITokenService tokenService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.tokenService = tokenService;
    }

    public async Task<GetTokenOkResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        await this.validator.ValidateAndThrowAsync(request, cancellationToken);

        var token = await this.tokenService.RefreshAsync(request.Dto.RefreshToken, cancellationToken);

        return this.mapper.Map<GetTokenOkResponseDto>(token);
    }
}
