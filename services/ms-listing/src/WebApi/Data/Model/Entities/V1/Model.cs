using WebApi.Data.Shared.Entities.V1.Base;

namespace WebApi.Data.Model.Entities.V1;

public sealed class Model : BaseEntity
{
    public const int NameMaxLength = 100;

    public Guid BrandId { get; set; }

    public Brand.Entities.V1.Brand Brand { get; set; } = null!;

    public string Name { get; set; } = string.Empty;
}
