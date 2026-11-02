namespace WebApi.Data.ListingImage.Dtos.V1;

public sealed record ReorderListingImagesDto
{
    public List<Guid>? ImageIds { get; init; }
}
