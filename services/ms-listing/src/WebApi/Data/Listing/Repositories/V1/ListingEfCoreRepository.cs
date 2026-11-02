using Microsoft.EntityFrameworkCore;
using WebApi.Data.ExchangeRate.Models.V1;
using WebApi.Data.Listing.Models.V1;
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

    public Task<PagedResponse<Entities.V1.Listing>> GetPageAsync(
        Guid? ownerId, bool withDeleted, int page, int pageSize, CancellationToken ct = default)
    {
        var listings = this.PageQuery(withDeleted);
        if (ownerId is not null)
        {
            listings = listings.Where(listing => listing.OwnerId == ownerId);
        }

        return ToPageAsync(listings, listings.NewestFirst(), page, pageSize, ct);
    }

    public Task<PagedResponse<Entities.V1.Listing>> SearchAsync(ListingSearch search, AznRates rates, CancellationToken ct = default)
    {
        var listings = this.PageQuery(withDeleted: false).Matching(search, rates);

        return ToPageAsync(listings, listings.SortedBy(search.Sort, rates), search.Page, search.PageSize, ct);
    }

    public Task<Entities.V1.Listing?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default)
    {
        return this.Query(withDeleted)
            .Include(listing => listing.Images.OrderBy(image => image.Position))
            .FirstOrDefaultAsync(listing => listing.Id == id, ct);
    }

    public Task<Guid?> GetOwnerIdAsync(Guid id, CancellationToken ct = default)
    {
        return this.context.Listings
            .Where(listing => listing.Id == id)
            .Select(listing => (Guid?)listing.OwnerId)
            .FirstOrDefaultAsync(ct);
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

    private static async Task<PagedResponse<Entities.V1.Listing>> ToPageAsync(
        IQueryable<Entities.V1.Listing> listings, IQueryable<Entities.V1.Listing> sorted, int page, int pageSize, CancellationToken ct)
    {
        var totalCount = await listings.CountAsync(ct);
        var items = await sorted.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return new PagedResponse<Entities.V1.Listing> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    // Only the cover of each listing: a page of 100 must not load 2,000 image rows.
    // Positions have no gaps, so the cover is at 0, which the (listing_id, position) index finds.
    private IQueryable<Entities.V1.Listing> PageQuery(bool withDeleted)
    {
        return this.Query(withDeleted)
            .Include(listing => listing.Images.Where(image => image.Position == 0))
            .AsNoTracking();
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
