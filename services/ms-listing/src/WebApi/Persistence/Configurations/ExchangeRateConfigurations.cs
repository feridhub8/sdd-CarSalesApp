using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WebApi.Data.ExchangeRate.Entities.V1;

namespace WebApi.Persistence.Configurations;

public sealed class ExchangeRateConfigurations : IEntityTypeConfiguration<ExchangeRate>
{
    public void Configure(EntityTypeBuilder<ExchangeRate> builder)
    {
        builder.HasKey(rate => rate.Currency);

        builder.Property(rate => rate.Currency).HasConversion<string>().HasMaxLength(3);
        builder.Property(rate => rate.RateToAzn).HasPrecision(12, 6);
    }
}
