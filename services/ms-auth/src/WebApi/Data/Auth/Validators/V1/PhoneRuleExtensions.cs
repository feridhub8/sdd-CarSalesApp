using System.Text.RegularExpressions;

namespace WebApi.Data.Auth.Validators.V1;

public static partial class PhoneRuleExtensions
{
    public const string InvalidMessage = "'phone' must be an Azerbaijani number: +994 and 9 digits.";

    // A number of any accepted form as it is stored (+994XXXXXXXXX), or null when it is not valid.
    // Operator codes are not checked, so a new code needs no release.
    public static string? NormalizePhone(this string? value)
    {
        var match = PhonePattern().Match(Separators().Replace(value ?? string.Empty, string.Empty));
        return match.Success ? $"+994{match.Groups[1].Value}" : null;
    }

    [GeneratedRegex(@"[\s\-()]")]
    private static partial Regex Separators();

    [GeneratedRegex(@"^(?:\+994|994|0)([0-9]{9})$")]
    private static partial Regex PhonePattern();
}
