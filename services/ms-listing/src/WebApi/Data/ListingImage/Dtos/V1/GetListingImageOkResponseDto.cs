namespace WebApi.Data.ListingImage.Dtos.V1;

public sealed record GetListingImageOkResponseDto
{
    public Guid Id { get; init; }

    public string Url { get; init; } = string.Empty;

    public string ThumbnailUrl { get; init; } = string.Empty;

    public int Position { get; init; }
}
