using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApi.Data.Model.Repositories.V1.Base;
using WebApi.Exceptions.V1;
using WebApi.Persistence;

namespace WebApi.Data.Model.Repositories.V1;

public sealed class ModelEfCoreRepository : IModelRepository
{
    private readonly ApplicationDbContext context;

    public ModelEfCoreRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public Task<List<Entities.V1.Model>> GetAllAsync(Guid? brandId, bool withDeleted, CancellationToken ct = default)
    {
        var models = this.Query(withDeleted).AsNoTracking();
        if (brandId is not null)
        {
            models = models.Where(model => model.BrandId == brandId);
        }

        return models.OrderBy(model => model.Name).ToListAsync(ct);
    }

    public Task<Entities.V1.Model?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default)
    {
        return this.Query(withDeleted).FirstOrDefaultAsync(model => model.Id == id, ct);
    }

    public Task<bool> NameExistsAsync(Guid brandId, string name, Guid? exceptId, CancellationToken ct = default)
    {
        // The column's collation makes this comparison ignore letter case; the query filter skips deleted models.
        return this.context.Models.AnyAsync(model => model.BrandId == brandId && model.Name == name && model.Id != exceptId, ct);
    }

    public Task<bool> HasActiveListingsAsync(Guid modelId, CancellationToken ct = default)
    {
        // The query filter skips deleted listings, so only listings people can see block a delete.
        return this.context.Listings.AnyAsync(listing => listing.ModelId == modelId, ct);
    }

    public async Task AddAsync(Entities.V1.Model model, CancellationToken ct = default)
    {
        this.context.Models.Add(model);
        await this.SaveChangesAsync(ct);
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            await this.context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Two requests can pass the name check at the same moment; the unique index still stops the second.
            throw new ConflictException("A model with this name already exists for this brand.");
        }
    }

    // The brand is always loaded: responses show its name.
    private IQueryable<Entities.V1.Model> Query(bool withDeleted)
    {
        var models = withDeleted ? this.context.Models.IgnoreQueryFilters() : this.context.Models;
        return models.Include(model => model.Brand);
    }
}
