using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Data.Listing.Entities.V1;

namespace WebApi.Persistence.Configurations;

public sealed class ListingConfigurations : IEntityTypeConfiguration<Listing>
{
    private const int EnumMaxLength = 20;

    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        // Enums are stored by name: readable in SQL, and adding or reordering values never rewrites rows.
        builder.Property(listing => listing.MileageUnit).HasConversion<string>().HasMaxLength(2);
        builder.Property(listing => listing.Currency).HasConversion<string>().HasMaxLength(3);
        builder.Property(listing => listing.BodyType).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(listing => listing.FuelType).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(listing => listing.Transmission).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(listing => listing.Drivetrain).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(listing => listing.Color).HasConversion<string>().HasMaxLength(EnumMaxLength);
        builder.Property(listing => listing.Market).HasConversion<string>().HasMaxLength(EnumMaxLength);

        builder.Property(listing => listing.EngineVolume).HasPrecision(3, 1);
        builder.Property(listing => listing.Price).HasPrecision(12, 2);
        builder.Property(listing => listing.Vin).HasMaxLength(Listing.VinLength);
        builder.Property(listing => listing.Description).HasMaxLength(Listing.DescriptionMaxLength);

        // Rows are never removed; model and city deletes are guarded by the active-listings rule instead.
        builder.HasOne(listing => listing.Model).WithMany().HasForeignKey(listing => listing.ModelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(listing => listing.City).WithMany().HasForeignKey(listing => listing.CityId).OnDelete(DeleteBehavior.Restrict);

        // Public paging (newest first) and an owner's own listings, both over non-deleted rows only.
        builder.HasIndex(listing => new { listing.CreatedAt, listing.Id })
            .IsDescending(true, true)
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_listings_created_at_id");
        builder.HasIndex(listing => new { listing.OwnerId, listing.CreatedAt })
            .IsDescending(false, true)
            .HasFilter("is_deleted = false")
            .HasDatabaseName("ix_listings_owner_id_created_at");
    }
}
