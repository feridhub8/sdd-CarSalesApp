using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.Listing.Services.V1.Base;

public interface IListingService
{
    Task<PagedResponse<Entities.V1.Listing>> GetPageAsync(int page, int pageSize, bool withDeleted, CancellationToken ct = default);

    Task<PagedResponse<Entities.V1.Listing>> GetMineAsync(int page, int pageSize, CancellationToken ct = default);

    Task<Entities.V1.Listing> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default);

    Task<Entities.V1.Listing> CreateAsync(Entities.V1.Listing listing, CancellationToken ct = default);

    Task<Entities.V1.Listing> GetForUpdateAsync(Guid id, CancellationToken ct = default);

    Task<Entities.V1.Listing> UpdateAsync(Entities.V1.Listing listing, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);

    Task RestoreAsync(Guid id, CancellationToken ct = default);
}
