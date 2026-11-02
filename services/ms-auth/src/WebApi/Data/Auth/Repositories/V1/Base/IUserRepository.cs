using WebApi.Data.Auth.Models.V1;

namespace WebApi.Data.Auth.Repositories.V1.Base;

public interface IUserRepository
{
    Task<Guid> CreateAsync(KeycloakUserRepresentation user, CancellationToken ct = default);

    Task<KeycloakUserRepresentation?> GetByIdAsync(Guid id, CancellationToken ct = default);

    // Returns false when the user does not exist.
    Task<bool> UpdatePhoneAsync(Guid id, string phone, CancellationToken ct = default);
}
