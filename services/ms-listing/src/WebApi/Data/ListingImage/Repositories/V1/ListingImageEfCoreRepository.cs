using Microsoft.EntityFrameworkCore;
using WebApi.Data.ListingImage.Repositories.V1.Base;
using WebApi.Persistence;

namespace WebApi.Data.ListingImage.Repositories.V1;

public sealed class ListingImageEfCoreRepository : IListingImageRepository
{
    private readonly ApplicationDbContext context;

    public ListingImageEfCoreRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public Task<Entities.V1.ListingImage?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return this.context.ListingImages
            .AsNoTracking()
            .Include(image => image.Listing)
            .FirstOrDefaultAsync(image => image.Id == id, ct);
    }

    public Task<int> CountAsync(Guid listingId, CancellationToken ct = default)
    {
        return this.context.ListingImages.CountAsync(image => image.ListingId == listingId, ct);
    }

    public Task<List<Entities.V1.ListingImage>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default)
    {
        return this.context.ListingImages
            .Where(image => image.ListingId == listingId)
            .OrderBy(image => image.Position)
            .ToListAsync(ct);
    }

    public void Add(Entities.V1.ListingImage image)
    {
        this.context.ListingImages.Add(image);
    }

    public void Remove(Entities.V1.ListingImage image)
    {
        this.context.ListingImages.Remove(image);
    }

    public async Task<bool> TryInLockedListingAsync(Guid listingId, Func<Task> action, CancellationToken ct = default)
    {
        await using var transaction = await this.context.Database.BeginTransactionAsync(ct);

        var locked = await this.context.Database
            .SqlQuery<Guid>($"""SELECT id AS "Value" FROM listings WHERE id = {listingId} AND is_deleted = false FOR UPDATE""")
            .ToListAsync(ct);
        if (locked.Count == 0)
        {
            return false;
        }

        await action();

        await this.context.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return true;
    }
}
