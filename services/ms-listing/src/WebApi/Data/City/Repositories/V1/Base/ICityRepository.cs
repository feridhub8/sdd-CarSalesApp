namespace WebApi.Data.City.Repositories.V1.Base;

public interface ICityRepository
{
    Task<List<Entities.V1.City>> GetAllAsync(bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.City?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default);

    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken ct = default);

    Task AddAsync(Entities.V1.City city, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
