namespace WebApi.Data.ListingImage.Entities.V1;

// Not a BaseEntity: images are public, so deleting one removes it for good instead of soft deleting it.
public sealed class ListingImage
{
    public const int MaxPerListing = 20;
    public const long MaxFileBytes = 10 * 1024 * 1024;

    // Version 7, like BaseEntity. Also names the image's files in storage.
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public Guid ListingId { get; set; }

    public Listing.Entities.V1.Listing Listing { get; set; } = null!;

    // From 0 without gaps; the image at 0 is the listing's cover.
    public int Position { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string LargeKey => $"{this.Id}/large.webp";

    public string ThumbnailKey => $"{this.Id}/thumbnail.webp";
}
