using WebApi.Data.Auth.Models.V1;
using WebApi.Data.Auth.Repositories.V1.Base;
using WebApi.Data.Auth.Services.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Auth.Services.V1;

public sealed class UserService : IUserService
{
    private readonly IUserRepository userRepository;

    public UserService(IUserRepository userRepository)
    {
        this.userRepository = userRepository;
    }

    public async Task<KeycloakUserRepresentation> RegisterAsync(KeycloakUserRepresentation user, CancellationToken ct = default)
    {
        var id = await this.userRepository.CreateAsync(user, ct);

        // Read the user back so the response shows what Keycloak stored (e.g. the email in lowercase).
        return await this.userRepository.GetByIdAsync(id, ct)
            ?? throw new InvalidOperationException($"User {id} was created but could not be read back.");
    }

    public async Task<KeycloakUserRepresentation> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await this.userRepository.GetByIdAsync(id, ct)
            ?? throw new UnauthorizedException("The user of this token no longer exists.");
    }
}
