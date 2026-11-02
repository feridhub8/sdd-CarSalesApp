using WebApi.Data.Listing.Enums.V1;

namespace WebApi.Data.ExchangeRate.Entities.V1;

// Not a BaseEntity: one row per currency, replaced in place and never deleted.
public sealed class ExchangeRate
{
    public Currency Currency { get; set; }

    // How many AZN one unit of the currency is worth.
    public decimal RateToAzn { get; set; }

    // The working day the source published this rate for.
    public DateOnly RateDate { get; set; }

    public DateTime UpdatedAt { get; set; }
}
