namespace WebApi.Data.Brand.Repositories.V1.Base;

public interface IBrandRepository
{
    Task<List<Entities.V1.Brand>> GetAllAsync(bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.Brand?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default);

    Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken ct = default);

    Task<bool> HasActiveModelsAsync(Guid brandId, CancellationToken ct = default);

    Task AddAsync(Entities.V1.Brand brand, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
