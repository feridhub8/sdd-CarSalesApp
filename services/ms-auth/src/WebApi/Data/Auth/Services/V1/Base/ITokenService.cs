using WebApi.Data.Auth.Models.V1;

namespace WebApi.Data.Auth.Services.V1.Base;

public interface ITokenService
{
    Task<KeycloakTokenResponse> LoginAsync(string email, string password, CancellationToken ct = default);

    Task<KeycloakTokenResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);

    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
}
