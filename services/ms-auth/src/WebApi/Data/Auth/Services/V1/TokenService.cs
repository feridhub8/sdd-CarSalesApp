using WebApi.Data.Auth.Models.V1;
using WebApi.Data.Auth.Repositories.V1.Base;
using WebApi.Data.Auth.Services.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Auth.Services.V1;

public sealed class TokenService : ITokenService
{
    private readonly ITokenRepository tokenRepository;

    public TokenService(ITokenRepository tokenRepository)
    {
        this.tokenRepository = tokenRepository;
    }

    public Task<KeycloakTokenResponse> LoginAsync(string login, string password, CancellationToken ct = default)
    {
        return this.tokenRepository.GetByPasswordAsync(login, password, ct);
    }

    public Task<KeycloakTokenResponse> RefreshAsync(string refreshToken, CancellationToken ct = default)
    {
        return this.tokenRepository.RefreshAsync(refreshToken, ct);
    }

    public async Task LogoutAsync(string refreshToken, CancellationToken ct = default)
    {
        // Keycloak answers 204 even for a session that already ended, so check the token first.
        if (!await this.tokenRepository.IsActiveAsync(refreshToken, ct))
        {
            throw new UnauthorizedException("The refresh token is invalid or expired.");
        }

        await this.tokenRepository.RevokeAsync(refreshToken, ct);
    }
}
