namespace WebApi.Data.City.Services.V1.Base;

public interface ICityService
{
    Task<List<Entities.V1.City>> GetAllAsync(bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.City> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.City> CreateAsync(Entities.V1.City city, CancellationToken ct = default);

    Task<Entities.V1.City> UpdateAsync(Guid id, string name, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task RestoreAsync(Guid id, CancellationToken ct = default);
}
