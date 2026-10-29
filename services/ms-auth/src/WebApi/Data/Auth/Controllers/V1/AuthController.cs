using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using WebApi.Data.Auth.Commands.V1;
using WebApi.Data.Auth.Dtos.V1;
using WebApi.Data.Auth.Enums.V1;
using WebApi.Data.Auth.Models.V1;
using WebApi.Data.Auth.Queries.V1;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Auth.Controllers.V1;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController : ControllerBase
{
    private const string SubjectClaim = "sub";
    private const string ClientTypeHeader = "X-Client-Type";
    private const string MobileClient = "mobile";

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
    public async Task<IActionResult> Login(
        [FromBody] LoginUserDto dto,
        [FromHeader(Name = ClientTypeHeader)] string? clientType,
        CancellationToken ct)
    {
        var token = await this.mediator.Send(new LoginUserCommand(dto), ct);
        return this.Ok(this.DeliverToken(token, ParseClientType(clientType)));
    }

    [HttpPost("Refresh")]
    [ProducesResponseType<GetTokenOkResponseDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] RefreshTokenDto? dto,
        [FromHeader(Name = ClientTypeHeader)] string? clientType,
        CancellationToken ct)
    {
        var client = ParseClientType(clientType);
        var refreshToken = this.ReadRefreshToken(client, dto?.RefreshToken);

        try
        {
            var token = await this.mediator.Send(new RefreshTokenCommand(new RefreshTokenDto { RefreshToken = refreshToken }), ct);
            return this.Ok(this.DeliverToken(token, client));
        }
        catch (UnauthorizedException) when (client == ClientType.Web)
        {
            this.Response.Cookies.Delete(RefreshTokenCookie.Name, RefreshTokenCookie.DeleteOptions());
            throw;
        }
    }

    [HttpPost("Logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(
        [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] LogoutUserDto? dto,
        [FromHeader(Name = ClientTypeHeader)] string? clientType,
        CancellationToken ct)
    {
        var client = ParseClientType(clientType);

        try
        {
            var refreshToken = this.ReadRefreshToken(client, dto?.RefreshToken);
            await this.mediator.Send(new LogoutUserCommand(new LogoutUserDto { RefreshToken = refreshToken }), ct);
            return this.NoContent();
        }
        finally
        {
            // A web logout always clears the cookie, even when the token was already dead.
            if (client == ClientType.Web)
            {
                this.Response.Cookies.Delete(RefreshTokenCookie.Name, RefreshTokenCookie.DeleteOptions());
            }
        }
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

    private static ClientType ParseClientType(string? clientType)
    {
        return string.Equals(clientType, MobileClient, StringComparison.OrdinalIgnoreCase) ? ClientType.Mobile : ClientType.Web;
    }

    // Isolation rule: web reads the refresh token only from the cookie, mobile only from the body.
    // Never fall back from one source to the other: if a mobile-style request could use the cookie,
    // injected JavaScript could get the cookie's token back in the response body.
    private string ReadRefreshToken(ClientType client, string? bodyToken)
    {
        if (client == ClientType.Mobile)
        {
            // Empty is left to the validator, which answers 400.
            return bodyToken ?? string.Empty;
        }

        return this.Request.Cookies.TryGetValue(RefreshTokenCookie.Name, out var cookieToken) && !string.IsNullOrEmpty(cookieToken)
            ? cookieToken
            : throw new UnauthorizedException("No refresh token cookie was sent.");
    }

    private GetTokenOkResponseDto DeliverToken(GetTokenOkResponseDto token, ClientType client)
    {
        if (client == ClientType.Mobile)
        {
            return token;
        }

        this.Response.Cookies.Append(RefreshTokenCookie.Name, token.RefreshToken!, RefreshTokenCookie.CreateOptions(token.RefreshExpiresIn));
        return token with { RefreshToken = null };
    }
}
