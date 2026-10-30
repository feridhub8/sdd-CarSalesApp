using WebApi.Data.Shared.Entities.V1.Base;

namespace WebApi.Data.City.Entities.V1;

public sealed class City : BaseEntity
{
    public const int NameMaxLength = 100;

    public string Name { get; set; } = string.Empty;
}
