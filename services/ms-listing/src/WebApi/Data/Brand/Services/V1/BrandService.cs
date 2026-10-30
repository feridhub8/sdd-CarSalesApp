using WebApi.Data.Brand.Repositories.V1.Base;
using WebApi.Data.Brand.Services.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Brand.Services.V1;

public sealed class BrandService : IBrandService
{
    private readonly IBrandRepository brandRepository;

    public BrandService(IBrandRepository brandRepository)
    {
        this.brandRepository = brandRepository;
    }

    public Task<List<Entities.V1.Brand>> GetAllAsync(bool withDeleted, CancellationToken ct = default)
    {
        return this.brandRepository.GetAllAsync(withDeleted, ct);
    }

    public async Task<Entities.V1.Brand> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default)
    {
        return await this.brandRepository.GetByIdAsync(id, withDeleted, ct)
            ?? throw new NotFoundException($"Brand {id} was not found.");
    }

    public async Task<Entities.V1.Brand> CreateAsync(Entities.V1.Brand brand, CancellationToken ct = default)
    {
        await this.EnsureNameIsFreeAsync(brand.Name, exceptId: null, ct);
        await this.brandRepository.AddAsync(brand, ct);
        return brand;
    }

    public async Task<Entities.V1.Brand> UpdateAsync(Guid id, string name, CancellationToken ct = default)
    {
        var brand = await this.GetByIdAsync(id, withDeleted: false, ct);
        await this.EnsureNameIsFreeAsync(name, exceptId: id, ct);

        brand.Name = name;
        await this.brandRepository.SaveChangesAsync(ct);
        return brand;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var brand = await this.GetByIdAsync(id, withDeleted: false, ct);
        if (await this.brandRepository.HasActiveModelsAsync(id, ct))
        {
            throw new ConflictException($"Brand {id} still has models; delete them first.");
        }

        brand.IsDeleted = true;
        await this.brandRepository.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct = default)
    {
        var brand = await this.GetByIdAsync(id, withDeleted: true, ct);
        if (!brand.IsDeleted)
        {
            throw new ConflictException($"Brand {id} is not deleted.");
        }

        await this.EnsureNameIsFreeAsync(brand.Name, exceptId: id, ct);

        brand.IsDeleted = false;
        await this.brandRepository.SaveChangesAsync(ct);
    }

    private async Task EnsureNameIsFreeAsync(string name, Guid? exceptId, CancellationToken ct)
    {
        if (await this.brandRepository.NameExistsAsync(name, exceptId, ct))
        {
            throw new ConflictException($"A brand named '{name}' already exists.");
        }
    }
}
