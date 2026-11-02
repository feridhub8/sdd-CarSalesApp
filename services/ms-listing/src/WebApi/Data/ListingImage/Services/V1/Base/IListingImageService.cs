namespace WebApi.Data.ListingImage.Services.V1.Base;

public interface IListingImageService
{
    Task<Entities.V1.ListingImage> CreateAsync(Guid listingId, Stream file, CancellationToken ct = default);

    Task<List<Entities.V1.ListingImage>> ReorderAsync(Guid listingId, IReadOnlyList<Guid> imageIds, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
