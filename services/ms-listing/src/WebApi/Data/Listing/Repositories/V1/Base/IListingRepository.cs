using WebApi.Data.ExchangeRate.Models.V1;
using WebApi.Data.Listing.Models.V1;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Data.Listing.Repositories.V1.Base;

public interface IListingRepository
{
    Task<PagedResponse<Entities.V1.Listing>> GetPageAsync(Guid? ownerId, bool withDeleted, int page, int pageSize, CancellationToken ct = default);

    Task<PagedResponse<Entities.V1.Listing>> SearchAsync(ListingSearch search, AznRates rates, CancellationToken ct = default);

    Task<Entities.V1.Listing?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default);

    // The owner of a non-deleted listing, without loading or tracking the listing.
    Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default);

    Task AddAsync(Entities.V1.Listing listing, CancellationToken ct = default);

    Task SaveChangesAsync(CancellationToken ct = default);
}
