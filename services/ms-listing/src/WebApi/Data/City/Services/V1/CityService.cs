using WebApi.Data.City.Repositories.V1.Base;
using WebApi.Data.City.Services.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.City.Services.V1;

public sealed class CityService : ICityService
{
    private readonly ICityRepository cityRepository;

    public CityService(ICityRepository cityRepository)
    {
        this.cityRepository = cityRepository;
    }

    public Task<List<Entities.V1.City>> GetAllAsync(bool withDeleted, CancellationToken ct = default)
    {
        return this.cityRepository.GetAllAsync(withDeleted, ct);
    }

    public async Task<Entities.V1.City> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default)
    {
        return await this.cityRepository.GetByIdAsync(id, withDeleted, ct)
            ?? throw new NotFoundException($"City {id} was not found.");
    }

    public async Task<Entities.V1.City> CreateAsync(Entities.V1.City city, CancellationToken ct = default)
    {
        await this.EnsureNameIsFreeAsync(city.Name, exceptId: null, ct);
        await this.cityRepository.AddAsync(city, ct);
        return city;
    }

    public async Task<Entities.V1.City> UpdateAsync(Guid id, string name, CancellationToken ct = default)
    {
        var city = await this.GetByIdAsync(id, withDeleted: false, ct);
        await this.EnsureNameIsFreeAsync(name, exceptId: id, ct);

        city.Name = name;
        await this.cityRepository.SaveChangesAsync(ct);
        return city;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var city = await this.GetByIdAsync(id, withDeleted: false, ct);
        if (await this.cityRepository.HasActiveListingsAsync(id, ct))
        {
            throw new ConflictException($"City {id} is used by listings; delete or move them first.");
        }

        city.IsDeleted = true;
        await this.cityRepository.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct = default)
    {
        var city = await this.GetByIdAsync(id, withDeleted: true, ct);
        if (!city.IsDeleted)
        {
            throw new ConflictException($"City {id} is not deleted.");
        }

        await this.EnsureNameIsFreeAsync(city.Name, exceptId: id, ct);

        city.IsDeleted = false;
        await this.cityRepository.SaveChangesAsync(ct);
    }

    private async Task EnsureNameIsFreeAsync(string name, Guid? exceptId, CancellationToken ct)
    {
        if (await this.cityRepository.NameExistsAsync(name, exceptId, ct))
        {
            throw new ConflictException($"A city named '{name}' already exists.");
        }
    }
}
