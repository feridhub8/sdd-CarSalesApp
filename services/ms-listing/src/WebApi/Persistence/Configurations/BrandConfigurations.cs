using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Data.Brand.Entities.V1;

namespace WebApi.Persistence.Configurations;

public sealed class BrandConfigurations : IEntityTypeConfiguration<Brand>
{
    public void Configure(EntityTypeBuilder<Brand> builder)
    {
        builder.Property(brand => brand.Name)
            .HasMaxLength(Brand.NameMaxLength)
            .UseCollation(ApplicationDbContext.CaseInsensitiveCollation);

        // Unique among non-deleted brands only, so a deleted brand's name can be used again.
        builder.HasIndex(brand => brand.Name).IsUnique().HasFilter("is_deleted = false");
    }
}
