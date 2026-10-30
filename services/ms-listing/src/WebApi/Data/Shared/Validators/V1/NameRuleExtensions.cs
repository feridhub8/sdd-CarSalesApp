using FluentValidation;

namespace WebApi.Data.Shared.Validators.V1;

public static class NameRuleExtensions
{
    // Catalog names: required and at most maxLength characters once surrounding spaces are trimmed.
    public static IRuleBuilderOptions<T, string> EntryName<T>(this IRuleBuilder<T, string> rule, int maxLength)
    {
        return rule
            .Must(name => !string.IsNullOrWhiteSpace(name))
            .WithMessage("'{PropertyName}' must not be empty.")
            .Must(name => (name?.Trim().Length ?? 0) <= maxLength)
            .WithMessage($"'{{PropertyName}}' must be {maxLength} characters or fewer.");
    }
}
