using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using WebApi.Data.Brand.Entities.V1;
using WebApi.Data.City.Entities.V1;
using WebApi.Data.Model.Entities.V1;
using WebApi.Data.Shared.Entities.V1.Base;

namespace WebApi.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    // Names compare ignoring letter case, so unique indexes treat "BMW" and "bmw" as the same name.
    public const string CaseInsensitiveCollation = "case_insensitive";

    public DbSet<Brand> Brands => this.Set<Brand>();

    public DbSet<Model> Models => this.Set<Model>();

    public DbSet<City> Cities => this.Set<City>();

    public override Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in this.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;

                if (entry.Property(entity => entity.IsDeleted).IsModified)
                {
                    entry.Entity.DeletedAt = entry.Entity.IsDeleted ? now : null;
                }
            }
        }

        return base.SaveChangesAsync(ct);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasCollation(CaseInsensitiveCollation, locale: "und-u-ks-level2", provider: "icu", deterministic: false);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Soft delete: normal queries never see deleted rows; WithDeleted reads use IgnoreQueryFilters().
        foreach (var entityType in modelBuilder.Model.GetEntityTypes().Where(type => typeof(BaseEntity).IsAssignableFrom(type.ClrType)))
        {
            var entity = Expression.Parameter(entityType.ClrType, "entity");
            var notDeleted = Expression.Lambda(Expression.Not(Expression.Property(entity, nameof(BaseEntity.IsDeleted))), entity);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(notDeleted);
        }
    }
}
