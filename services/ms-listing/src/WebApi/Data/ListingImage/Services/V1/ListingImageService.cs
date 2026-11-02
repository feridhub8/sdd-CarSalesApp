using FluentValidation;
using FluentValidation.Results;
using WebApi.Data.Listing.Repositories.V1.Base;
using WebApi.Data.ListingImage.Repositories.V1.Base;
using WebApi.Data.ListingImage.Services.V1.Base;
using WebApi.Data.Shared.Repositories.V1.Base;
using WebApi.Data.Shared.Services.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.ListingImage.Services.V1;

// Files and rows cannot share a transaction, so each flow orders its steps for the safer failure.
public sealed class ListingImageService : IListingImageService
{
    private readonly IListingImageRepository imageRepository;
    private readonly IListingRepository listingRepository;
    private readonly IFileRepository fileRepository;
    private readonly IImageProcessingService imageProcessing;
    private readonly ICurrentUserService currentUser;
    private readonly ILogger<ListingImageService> logger;

    public ListingImageService(
        IListingImageRepository imageRepository,
        IListingRepository listingRepository,
        IFileRepository fileRepository,
        IImageProcessingService imageProcessing,
        ICurrentUserService currentUser,
        ILogger<ListingImageService> logger)
    {
        this.imageRepository = imageRepository;
        this.listingRepository = listingRepository;
        this.fileRepository = fileRepository;
        this.imageProcessing = imageProcessing;
        this.currentUser = currentUser;
        this.logger = logger;
    }

    public async Task<Entities.V1.ListingImage> CreateAsync(Guid listingId, Stream file, CancellationToken ct = default)
    {
        await this.EnsureOwnerAsync(listingId, ct);
        await this.EnsureRoomAsync(listingId, ct);

        var processed = await this.imageProcessing.ProcessAsync(file, ct);
        var image = new Entities.V1.ListingImage { ListingId = listingId };

        // Files first, row last: the slow work holds no lock, and a failure leaves no row without files.
        try
        {
            await this.UploadAsync(image.LargeKey, processed.Large, ct);
            await this.UploadAsync(image.ThumbnailKey, processed.Thumbnail, ct);

            var saved = await this.imageRepository.TryInLockedListingAsync(
                listingId,
                async () =>
                {
                    // Counted again under the lock: parallel uploads may have used the last places.
                    image.Position = await this.EnsureRoomAsync(listingId, ct);
                    this.imageRepository.Add(image);
                },
                ct);

            if (!saved)
            {
                throw ListingNotFound(listingId);
            }
        }
        catch
        {
            await this.DeleteFilesAsync(image);
            throw;
        }

        return image;
    }

    public async Task<List<Entities.V1.ListingImage>> ReorderAsync(Guid listingId, IReadOnlyList<Guid> imageIds, CancellationToken ct = default)
    {
        await this.EnsureOwnerAsync(listingId, ct);

        var images = new List<Entities.V1.ListingImage>();
        var saved = await this.imageRepository.TryInLockedListingAsync(
            listingId,
            async () =>
            {
                var current = await this.imageRepository.GetByListingIdAsync(listingId, ct);

                // Every image exactly once: one id set equal to the listing's, with no repeats.
                if (imageIds.Count != current.Count || !imageIds.ToHashSet().SetEquals(current.Select(image => image.Id)))
                {
                    throw new ValidationException([new ValidationFailure("imageIds", "Send every image id of the listing exactly once.")]);
                }

                images = imageIds.Select(id => current.Single(image => image.Id == id)).ToList();
                for (var position = 0; position < images.Count; position++)
                {
                    images[position].Position = position;
                }
            },
            ct);

        return saved ? images : throw ListingNotFound(listingId);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var image = await this.imageRepository.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Image {id} was not found.");

        if (image.Listing.OwnerId != this.currentUser.Id && !this.currentUser.IsAdmin)
        {
            throw new ForbiddenException("Only the owner or an admin can delete this image.");
        }

        // Row first, files last: once this commits the image is gone from every response.
        var saved = await this.imageRepository.TryInLockedListingAsync(
            image.ListingId,
            async () =>
            {
                var current = await this.imageRepository.GetByListingIdAsync(image.ListingId, ct);
                var target = current.SingleOrDefault(candidate => candidate.Id == id)
                    ?? throw new NotFoundException($"Image {id} was not found.");

                this.imageRepository.Remove(target);

                var remaining = current.Where(candidate => candidate.Id != id).ToList();
                for (var position = 0; position < remaining.Count; position++)
                {
                    remaining[position].Position = position;
                }
            },
            ct);

        if (!saved)
        {
            throw new NotFoundException($"Image {id} was not found.");
        }

        await this.DeleteFilesAsync(image);
    }

    // Missing or deleted listings give 404 before ownership is checked, as for listing updates.
    private async Task EnsureOwnerAsync(Guid listingId, CancellationToken ct)
    {
        var ownerId = await this.listingRepository.GetOwnerIdAsync(listingId, ct)
            ?? throw ListingNotFound(listingId);

        if (ownerId != this.currentUser.Id)
        {
            throw new ForbiddenException("Only the owner can change the images of this listing.");
        }
    }

    // Returns the current count, which is also the position of a new image.
    private async Task<int> EnsureRoomAsync(Guid listingId, CancellationToken ct)
    {
        var count = await this.imageRepository.CountAsync(listingId, ct);
        if (count >= Entities.V1.ListingImage.MaxPerListing)
        {
            throw new ConflictException($"A listing can have at most {Entities.V1.ListingImage.MaxPerListing} images.");
        }

        return count;
    }

    private async Task UploadAsync(string key, byte[] content, CancellationToken ct)
    {
        using var stream = new MemoryStream(content);
        await this.fileRepository.UploadAsync(key, stream, this.imageProcessing.ContentType, ct);
    }

    // Not cancellable and never failing: it runs after the outcome is decided, including after a client left.
    private async Task DeleteFilesAsync(Entities.V1.ListingImage image)
    {
        foreach (var key in new[] { image.LargeKey, image.ThumbnailKey })
        {
            try
            {
                await this.fileRepository.DeleteAsync(key, CancellationToken.None);
            }
            catch (Exception exception)
            {
                this.logger.LogWarning(exception, "Could not delete file {Key}; if it was stored, it stays readable by its URL.", key);
            }
        }
    }

    private static NotFoundException ListingNotFound(Guid listingId)
    {
        return new NotFoundException($"Listing {listingId} was not found.");
    }
}
