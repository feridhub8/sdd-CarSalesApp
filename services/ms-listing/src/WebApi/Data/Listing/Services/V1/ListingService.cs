using FluentValidation;
using FluentValidation.Results;
using WebApi.Data.City.Repositories.V1.Base;
using WebApi.Data.Listing.Repositories.V1.Base;
using WebApi.Data.Listing.Services.V1.Base;
using WebApi.Data.Model.Repositories.V1.Base;
using WebApi.Data.Shared.Models.V1;
using WebApi.Data.Shared.Services.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Listing.Services.V1;

public sealed class ListingService : IListingService
{
    private readonly IListingRepository listingRepository;
    private readonly IModelRepository modelRepository;
    private readonly ICityRepository cityRepository;
    private readonly ICurrentUserService currentUser;

    public ListingService(
        IListingRepository listingRepository,
        IModelRepository modelRepository,
        ICityRepository cityRepository,
        ICurrentUserService currentUser)
    {
        this.listingRepository = listingRepository;
        this.modelRepository = modelRepository;
        this.cityRepository = cityRepository;
        this.currentUser = currentUser;
    }

    public Task<PagedResponse<Entities.V1.Listing>> GetPageAsync(int page, int pageSize, bool withDeleted, CancellationToken ct = default)
    {
        return this.listingRepository.GetPageAsync(ownerId: null, withDeleted, page, pageSize, ct);
    }

    public Task<PagedResponse<Entities.V1.Listing>> GetMineAsync(int page, int pageSize, CancellationToken ct = default)
    {
        return this.listingRepository.GetPageAsync(this.currentUser.Id, withDeleted: false, page, pageSize, ct);
    }

    public async Task<Entities.V1.Listing> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default)
    {
        return await this.listingRepository.GetByIdAsync(id, withDeleted, ct)
            ?? throw new NotFoundException($"Listing {id} was not found.");
    }

    public async Task<Entities.V1.Listing> CreateAsync(Entities.V1.Listing listing, CancellationToken ct = default)
    {
        // The owner always comes from the access token, never from the request body.
        listing.OwnerId = this.currentUser.Id;
        await this.EnsureReferencesAsync(listing.ModelId, listing.CityId, ct);
        await this.listingRepository.AddAsync(listing, ct);

        // Read back with brand, model, and city, whose names are part of the response.
        return await this.GetByIdAsync(listing.Id, withDeleted: false, ct);
    }

    // Missing or deleted listings give 404 before ownership is checked, so callers cannot probe other users' listings.
    public async Task<Entities.V1.Listing> GetForUpdateAsync(Guid id, CancellationToken ct = default)
    {
        var listing = await this.GetByIdAsync(id, withDeleted: false, ct);
        if (listing.OwnerId != this.currentUser.Id)
        {
            throw new ForbiddenException("Only the owner can edit this listing.");
        }

        return listing;
    }

    public async Task<Entities.V1.Listing> UpdateAsync(Entities.V1.Listing listing, CancellationToken ct = default)
    {
        await this.EnsureReferencesAsync(listing.ModelId, listing.CityId, ct);
        await this.listingRepository.SaveChangesAsync(ct);

        return await this.GetByIdAsync(listing.Id, withDeleted: false, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var listing = await this.GetByIdAsync(id, withDeleted: false, ct);
        if (listing.OwnerId != this.currentUser.Id && !this.currentUser.IsAdmin)
        {
            throw new ForbiddenException("Only the owner or an admin can delete this listing.");
        }

        listing.IsDeleted = true;
        await this.listingRepository.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct = default)
    {
        var listing = await this.GetByIdAsync(id, withDeleted: true, ct);
        if (!listing.IsDeleted)
        {
            throw new ConflictException($"Listing {id} is not deleted.");
        }

        if (listing.Model.IsDeleted || listing.City.IsDeleted)
        {
            throw new ConflictException($"The model or city of listing {id} is deleted; restore it first.");
        }

        listing.IsDeleted = false;
        await this.listingRepository.SaveChangesAsync(ct);
    }

    // Missing references are wrong values in the request body, so they are reported on their fields.
    private async Task EnsureReferencesAsync(Guid modelId, Guid cityId, CancellationToken ct)
    {
        var failures = new List<ValidationFailure>();

        if (await this.modelRepository.GetByIdAsync(modelId, withDeleted: false, ct) is null)
        {
            failures.Add(new ValidationFailure("modelId", "The model does not exist or is deleted."));
        }

        if (await this.cityRepository.GetByIdAsync(cityId, withDeleted: false, ct) is null)
        {
            failures.Add(new ValidationFailure("cityId", "The city does not exist or is deleted."));
        }

        if (failures.Count > 0)
        {
            throw new ValidationException(failures);
        }
    }
}
