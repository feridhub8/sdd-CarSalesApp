using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApi.Data.Brand.Repositories.V1.Base;
using WebApi.Exceptions.V1;
using WebApi.Persistence;

namespace WebApi.Data.Brand.Repositories.V1;

public sealed class BrandEfCoreRepository : IBrandRepository
{
    private readonly ApplicationDbContext context;

    public BrandEfCoreRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public Task<List<Entities.V1.Brand>> GetAllAsync(bool withDeleted, CancellationToken ct = default)
    {
        return this.Query(withDeleted).AsNoTracking().OrderBy(brand => brand.Name).ToListAsync(ct);
    }

    public Task<Entities.V1.Brand?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default)
    {
        return this.Query(withDeleted).FirstOrDefaultAsync(brand => brand.Id == id, ct);
    }

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken ct = default)
    {
        // The column's collation makes this comparison ignore letter case; the query filter skips deleted brands.
        return this.context.Brands.AnyAsync(brand => brand.Name == name && brand.Id != exceptId, ct);
    }

    public Task<bool> HasActiveModelsAsync(Guid brandId, CancellationToken ct = default)
    {
        return this.context.Models.AnyAsync(model => model.BrandId == brandId, ct);
    }

    public async Task AddAsync(Entities.V1.Brand brand, CancellationToken ct = default)
    {
        this.context.Brands.Add(brand);
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
            throw new ConflictException("A brand with this name already exists.");
        }
    }

    private IQueryable<Entities.V1.Brand> Query(bool withDeleted)
    {
        return withDeleted ? this.context.Brands.IgnoreQueryFilters() : this.context.Brands;
    }
}
