using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Data.Model.Entities.V1;

namespace WebApi.Persistence.Configurations;

public sealed class ModelConfigurations : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.Property(model => model.Name)
            .HasMaxLength(Model.NameMaxLength)
            .UseCollation(ApplicationDbContext.CaseInsensitiveCollation);

        // Rows are never removed; a brand delete is guarded by the active-models rule instead of a cascade.
        builder.HasOne(model => model.Brand)
            .WithMany()
            .HasForeignKey(model => model.BrandId)
            .OnDelete(DeleteBehavior.Restrict);

        // Unique within a brand among non-deleted models, so "Golf" can exist under two brands.
        builder.HasIndex(model => new { model.BrandId, model.Name }).IsUnique().HasFilter("is_deleted = false");
    }
}
