namespace WebApi.Data.Shared.Repositories.V1.Base;

public interface ITokenRepository
{
    Task<bool> IsActiveAsync(string token, CancellationToken ct = default);
}
