namespace WebApi.Data.ListingImage.Repositories.V1.Base;

public interface IListingImageRepository
{
    // Without its listing being deleted; the result is not tracked.
    Task<Entities.V1.ListingImage?> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<int> CountAsync(Guid listingId, CancellationToken ct = default);

    // Ordered by position; the results are tracked, so changes reach the database on save.
    Task<List<Entities.V1.ListingImage>> GetByListingIdAsync(Guid listingId, CancellationToken ct = default);

    void Add(Entities.V1.ListingImage image);

    void Remove(Entities.V1.ListingImage image);

    // Runs the action in a transaction holding the listing's row lock, and saves and commits after it.
    // Writers of the same listing's images run one at a time, so counts and positions stay correct.
    // Returns false, without running the action, when the listing is missing or deleted.
    Task<bool> TryInLockedListingAsync(Guid listingId, Func<Task> action, CancellationToken ct = default);
}
