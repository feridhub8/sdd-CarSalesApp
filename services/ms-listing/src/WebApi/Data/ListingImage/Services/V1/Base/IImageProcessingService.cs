using WebApi.Data.ListingImage.Models.V1;

namespace WebApi.Data.ListingImage.Services.V1.Base;

public interface IImageProcessingService
{
    string ContentType { get; }

    // Throws a validation error on "file" when the content is not an accepted image.
    Task<ProcessedImage> ProcessAsync(Stream file, CancellationToken ct = default);
}
