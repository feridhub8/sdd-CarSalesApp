namespace WebApi.Data.Brand.Services.V1.Base;

public interface IBrandService
{
    Task<List<Entities.V1.Brand>> GetAllAsync(bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.Brand> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.Brand> CreateAsync(Entities.V1.Brand brand, CancellationToken ct = default);

    Task<Entities.V1.Brand> UpdateAsync(Guid id, string name, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task RestoreAsync(Guid id, CancellationToken ct = default);
}
