using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Data.ListingImage.Entities.V1;

namespace WebApi.Persistence.Configurations;

public sealed class ListingImageConfigurations : IEntityTypeConfiguration<ListingImage>
{
    public void Configure(EntityTypeBuilder<ListingImage> builder)
    {
        // Listing rows are never removed.
        builder.HasOne(image => image.Listing).WithMany(listing => listing.Images).HasForeignKey(image => image.ListingId).OnDelete(DeleteBehavior.Restrict);

        // Images of a deleted listing are hidden with it; WithDeleted reads use IgnoreQueryFilters().
        builder.HasQueryFilter(image => !image.Listing.IsDeleted);

        // Position is not unique: renumbering in a transaction would collide. Writers lock the listing row instead.
        builder.HasIndex(image => new { image.ListingId, image.Position })
            .HasDatabaseName("ix_listing_images_listing_id_position");
    }
}
