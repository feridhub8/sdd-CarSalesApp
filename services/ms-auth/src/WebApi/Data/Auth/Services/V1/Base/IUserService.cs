using WebApi.Data.Auth.Models.V1;

namespace WebApi.Data.Auth.Services.V1.Base;

public interface IUserService
{
    Task<KeycloakUserRepresentation> RegisterAsync(KeycloakUserRepresentation user, CancellationToken ct = default);

    Task<KeycloakUserRepresentation> GetByIdAsync(Guid id, CancellationToken ct = default);
}
