using FluentValidation;

namespace WebApi.Data.Shared.Validators.V1;

public static class PagingRuleExtensions
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPage = 500;
    public const int MaxPageSize = 100;

    // The page cap bounds the deepest OFFSET; deeper browsing belongs to search.
    public static IRuleBuilderOptions<T, int> PageNumber<T>(this IRuleBuilder<T, int> rule)
    {
        return rule.InclusiveBetween(1, MaxPage);
    }

    public static IRuleBuilderOptions<T, int> PageSize<T>(this IRuleBuilder<T, int> rule)
    {
        return rule.InclusiveBetween(1, MaxPageSize);
    }
}
