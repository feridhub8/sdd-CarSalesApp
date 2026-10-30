using FluentValidation;
using FluentValidation.Results;
using WebApi.Data.Brand.Repositories.V1.Base;
using WebApi.Data.Model.Repositories.V1.Base;
using WebApi.Data.Model.Services.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Model.Services.V1;

public sealed class ModelService : IModelService
{
    private readonly IModelRepository modelRepository;
    private readonly IBrandRepository brandRepository;

    public ModelService(IModelRepository modelRepository, IBrandRepository brandRepository)
    {
        this.modelRepository = modelRepository;
        this.brandRepository = brandRepository;
    }

    public Task<List<Entities.V1.Model>> GetAllAsync(Guid? brandId, bool withDeleted, CancellationToken ct = default)
    {
        return this.modelRepository.GetAllAsync(brandId, withDeleted, ct);
    }

    public async Task<Entities.V1.Model> GetByIdAsync(Guid id, bool withDeleted, CancellationToken ct = default)
    {
        return await this.modelRepository.GetByIdAsync(id, withDeleted, ct)
            ?? throw new NotFoundException($"Model {id} was not found.");
    }

    public async Task<Entities.V1.Model> CreateAsync(Entities.V1.Model model, CancellationToken ct = default)
    {
        await this.EnsureBrandIsActiveAsync(model.BrandId, ct);
        await this.EnsureNameIsFreeAsync(model.BrandId, model.Name, exceptId: null, ct);
        await this.modelRepository.AddAsync(model, ct);

        // Read back with the brand, whose name is part of the response.
        return await this.GetByIdAsync(model.Id, withDeleted: false, ct);
    }

    public async Task<Entities.V1.Model> UpdateAsync(Guid id, Guid brandId, string name, CancellationToken ct = default)
    {
        var model = await this.GetByIdAsync(id, withDeleted: false, ct);
        await this.EnsureBrandIsActiveAsync(brandId, ct);
        await this.EnsureNameIsFreeAsync(brandId, name, exceptId: id, ct);

        model.BrandId = brandId;
        model.Name = name;
        await this.modelRepository.SaveChangesAsync(ct);

        return await this.GetByIdAsync(id, withDeleted: false, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var model = await this.GetByIdAsync(id, withDeleted: false, ct);
        if (await this.modelRepository.HasActiveListingsAsync(id, ct))
        {
            throw new ConflictException($"Model {id} is used by listings; delete or move them first.");
        }

        model.IsDeleted = true;
        await this.modelRepository.SaveChangesAsync(ct);
    }

    public async Task RestoreAsync(Guid id, CancellationToken ct = default)
    {
        var model = await this.GetByIdAsync(id, withDeleted: true, ct);
        if (!model.IsDeleted)
        {
            throw new ConflictException($"Model {id} is not deleted.");
        }

        if (model.Brand.IsDeleted)
        {
            throw new ConflictException($"The brand of model {id} is deleted; restore the brand first.");
        }

        await this.EnsureNameIsFreeAsync(model.BrandId, model.Name, exceptId: id, ct);

        model.IsDeleted = false;
        await this.modelRepository.SaveChangesAsync(ct);
    }

    // A missing brand is a wrong value in the request body, so it is reported on the brandId field.
    private async Task EnsureBrandIsActiveAsync(Guid brandId, CancellationToken ct)
    {
        if (await this.brandRepository.GetByIdAsync(brandId, withDeleted: false, ct) is null)
        {
            throw new ValidationException([new ValidationFailure("brandId", "The brand does not exist or is deleted.")]);
        }
    }

    private async Task EnsureNameIsFreeAsync(Guid brandId, string name, Guid? exceptId, CancellationToken ct)
    {
        if (await this.modelRepository.NameExistsAsync(brandId, name, exceptId, ct))
        {
            throw new ConflictException($"This brand already has a model named '{name}'.");
        }
    }
}
