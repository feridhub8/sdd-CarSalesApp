namespace WebApi.Data.Model.Repositories.V1.Base;

public interface IModelRepository
{
    Task<List<Entities.V1.Model>> GetAllAsync(Guid? brandId, bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.Model?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default);

    Task<bool> NameExistsAsync(Guid brandId, string name, Guid? exceptId, CancellationToken ct = default);

    Task<bool> HasActiveListingsAsync(Guid modelId, CancellationToken ct = default);

    Task AddAsync(Entities.V1.Model model, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
