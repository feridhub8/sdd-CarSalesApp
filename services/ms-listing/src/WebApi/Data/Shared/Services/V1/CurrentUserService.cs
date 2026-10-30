using WebApi.Data.Shared.Models.V1;
using WebApi.Data.Shared.Services.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Shared.Services.V1;

// The signed-in user of the current request, read from the validated access token.
public sealed class CurrentUserService : ICurrentUserService
{
    private const string SubjectClaim = "sub";

    private readonly IHttpContextAccessor httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public Guid Id =>
        Guid.TryParse(this.httpContextAccessor.HttpContext?.User.FindFirst(SubjectClaim)?.Value, out var id)
            ? id
            : throw new UnauthorizedException("The access token has no valid subject.");

    public bool IsAdmin => this.httpContextAccessor.HttpContext?.User.IsInRole(Roles.Admin) == true;
}
