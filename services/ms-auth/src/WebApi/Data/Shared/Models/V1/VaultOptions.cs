namespace WebApi.Data.Shared.Models.V1;

public sealed class VaultOptions
{
    public const string SectionName = "Vault";

    public string Address { get; init; } = string.Empty;

    public string Mount { get; init; } = string.Empty;

    public string SecretPath { get; init; } = string.Empty;
}
