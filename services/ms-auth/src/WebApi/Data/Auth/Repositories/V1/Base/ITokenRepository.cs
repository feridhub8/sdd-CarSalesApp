using WebApi.Data.Auth.Models.V1;

namespace WebApi.Data.Auth.Repositories.V1.Base;

public interface ITokenRepository
{
    Task<KeycloakTokenResponse> GetByPasswordAsync(string login, string password, CancellationToken ct = default);

    Task<KeycloakTokenResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);

    Task RevokeAsync(string refreshToken, CancellationToken ct = default);

    Task<bool> IsActiveAsync(string token, CancellationToken ct = default);

    Task<KeycloakTokenResponse> GetServiceAccountTokenAsync(CancellationToken ct = default);
}
