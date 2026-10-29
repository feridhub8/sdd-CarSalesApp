using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Data.Auth.Commands.V1;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Queries.V1;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Auth.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController : ControllerBase
{
    private const string SubjectClaim = "sub";

    private readonly IMediator mediator;

    public AuthController(IMediator mediator)
    {
        this.mediator = mediator;
    }

    [HttpPost("Register")]
    [ProducesResponseType<GetUserOkResponseDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterUserDto dto, CancellationToken ct)
    {
        var user = await this.mediator.Send(new RegisterUserCommand(dto), ct);
        return this.StatusCode(StatusCodes.Status201Created, user);
    }

    [HttpPost("Login")]
    [ProducesResponseType<GetTokenOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginUserDto dto, CancellationToken ct)
    {
        return this.Ok(await this.mediator.Send(new LoginUserCommand(dto), ct));
    }

    [HttpPost("Refresh")]
    [ProducesResponseType<GetTokenOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenDto dto, CancellationToken ct)
    {
        return this.Ok(await this.mediator.Send(new RefreshTokenCommand(dto), ct));
    }

    [HttpPost("Logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] LogoutUserDto dto, CancellationToken ct)
    {
        await this.mediator.Send(new LogoutUserCommand(dto), ct);
        return this.NoContent();
    }

    [Authorize]
    [HttpGet("Me")]
    [ProducesResponseType<GetUserOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        if (!Guid.TryParse(this.User.FindFirst(SubjectClaim)?.Value, out var userId))
        {
            throw new UnauthorizedException("The access token has no valid subject.");
        }

        return this.Ok(await this.mediator.Send(new GetUserByIdQuery(userId), ct));
    }
}
