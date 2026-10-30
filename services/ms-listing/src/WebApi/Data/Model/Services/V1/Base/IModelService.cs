namespace WebApi.Data.Model.Services.V1.Base;

public interface IModelService
{
    Task<List<Entities.V1.Model>> GetAllAsync(Guid? brandId, bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.Model> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.Model> CreateAsync(Entities.V1.Model model, CancellationToken ct = default);

    Task<Entities.V1.Model> UpdateAsync(Guid id, Guid brandId, string name, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task RestoreAsync(Guid id, CancellationToken ct = default);
}
