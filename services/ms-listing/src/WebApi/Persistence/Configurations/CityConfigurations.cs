using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Data.City.Entities.V1;

namespace WebApi.Persistence.Configurations;

public sealed class CityConfigurations : IEntityTypeConfiguration<City>
{
    public void Configure(EntityTypeBuilder<City> builder)
    {
        builder.Property(city => city.Name)
            .HasMaxLength(City.NameMaxLength)
            .UseCollation(ApplicationDbContext.CaseInsensitiveCollation);

        // Unique among non-deleted cities only, so a deleted city's name can be used again.
        builder.HasIndex(city => city.Name).IsUnique().HasFilter("is_deleted = false");
    }
}
