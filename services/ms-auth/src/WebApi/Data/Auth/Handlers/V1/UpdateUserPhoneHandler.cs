using FluentValidation;
using MapsterMapper;
using MediatR;
using WebApi.Data.Auth.Commands.V1;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Services.V1.Base;
using WebApi.Data.Auth.Validators.V1;

namespace WebApi.Data.Auth.Handlers.V1;

public sealed class UpdateUserPhoneHandler : IRequestHandler<UpdateUserPhoneCommand, GetUserOkResponseDto>
{
    private readonly IValidator<UpdateUserPhoneCommand> validator;
    private readonly IMapper mapper;
    private readonly IUserService userService;

    public UpdateUserPhoneHandler(IValidator<UpdateUserPhoneCommand> validator, IMapper mapper, IUserService userService)
    {
        this.validator = validator;
        this.mapper = mapper;
        this.userService = userService;
    }

    public async Task<GetUserOkResponseDto> Handle(UpdateUserPhoneCommand request, CancellationToken ct)
    {
        await this.validator.ValidateAndThrowAsync(request, ct);

        var user = await this.userService.UpdatePhoneAsync(request.UserId, request.Dto.Phone.NormalizePhone()!, ct);

        return this.mapper.Map<GetUserOkResponseDto>(user);
    }
}
