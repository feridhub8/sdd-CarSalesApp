using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApi.Data.City.Repositories.V1.Base;
using WebApi.Exceptions.V1;
using WebApi.Persistence;

namespace WebApi.Data.City.Repositories.V1;

public sealed class CityEfCoreRepository : ICityRepository
{
    private readonly ApplicationDbContext context;

    public CityEfCoreRepository(ApplicationDbContext context)
    {
        this.context = context;
    }

    public Task<List<Entities.V1.City>> GetAllAsync(bool withDeleted, CancellationToken ct = default)
    {
        return this.Query(withDeleted).AsNoTracking().OrderBy(city => city.Name).ToListAsync(ct);
    }

    public Task<Entities.V1.City?> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default)
    {
        return this.Query(withDeleted).FirstOrDefaultAsync(city => city.Id == id, ct);
    }

    public Task<bool> NameExistsAsync(string name, Guid? exceptId, CancellationToken ct = default)
    {
        // The column's collation makes this comparison ignore letter case; the query filter skips deleted cities.
        return this.context.Cities.AnyAsync(city => city.Name == name && city.Id != exceptId, ct);
    }

    public Task<bool> HasActiveListingsAsync(Guid cityId, CancellationToken ct = default)
    {
        // The query filter skips deleted listings, so only listings people can see block a delete.
        return this.context.Listings.AnyAsync(listing => listing.CityId == cityId, ct);
    }

    public async Task AddAsync(Entities.V1.City city, CancellationToken ct = default)
    {
        this.context.Cities.Add(city);
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
            throw new ConflictException("A city with this name already exists.");
        }
    }

    private IQueryable<Entities.V1.City> Query(bool withDeleted)
    {
        return withDeleted ? this.context.Cities.IgnoreQueryFilters() : this.context.Cities;
    }
}
