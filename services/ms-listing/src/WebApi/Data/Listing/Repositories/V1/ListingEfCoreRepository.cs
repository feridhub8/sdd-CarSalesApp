using Microsoft.EntityFrameworkCore;
using WebApi.Data.Listing.Repositories.V1.Base;
using WebApi.Data.Shared.Models.V1;
using WebApi.Persistence;

namespace WebApi.Data.Listing.Repositories.V1;

public sealed class ListingEfCoreRepository : IListingRepository
{
    private readonly ApplicationDbContext context;

    public ListingEfCoreRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public async Task<PagedResponse<Entities.V1.Listing>> GetPageAsync(
        Guid? ownerId, bool withDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var listings = this.Query(withDeleted).AsNoTracking();
        if (ownerId is not null)
        {
            listings = listings.Where(listing => listing.OwnerId == ownerId);
        }

        var totalCount = await listings.CountAsync(ct);

        // Newest first; the id breaks ties so pages stay stable for listings created at the same instant.
        var items = await listings
            .OrderByDescending(listing => listing.CreatedAt)
            .ThenByDescending(listing => listing.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<Entities.V1.Listing> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public Task<Entities.V1.Listing?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default)
    {
        return this.Query(withDeleted).FirstOrDefaultAsync(listing => listing.Id == id, ct);
    }

    public async Task AddAsync(Entities.V1.Listing listing, CancellationToken ct = default)
    {
        this.context.Listings.Add(listing);
        await this.context.SaveChangesAsync(ct);
    }

    public Task SaveChangesAsync(CancellationToken ct = default)
    {
        return this.context.SaveChangesAsync(ct);
    }

    // Brand, model, and city are always loaded: responses show their names.
    private IQueryable<Entities.V1.Listing> Query(bool withDeleted)
    {
        var listings = withDeleted ? this.context.Listings.IgnoreQueryFilters() : this.context.Listings;
        return listings
            .Include(listing => listing.Model).ThenInclude(model => model.Brand)
            .Include(listing => listing.City);
    }
}
