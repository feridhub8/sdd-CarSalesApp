namespace WebApi.Data.ListingImage.Dtos.V1;

public sealed record CreateListingImageDto
{
    public IFormFile? File { get; init; }
}
