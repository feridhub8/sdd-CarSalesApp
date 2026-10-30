using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.Listing.Repositories.V1.Base;

public interface IListingRepository
{
    Task<PagedResponse<Entities.V1.Listing>> GetPageAsync(Guid? ownerId, bool withDeleted, int page, int pageSize, CancellationToken ct = default);

    Task<Entities.V1.Listing?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default);

    Task AddAsync(Entities.V1.Listing listing, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
