namespace WebApi.Data.Shared.Models.V1;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string Endpoint { get; init; } = string.Empty;

    public string Bucket { get; init; } = string.Empty;

    public string AccessKey { get; init; } = string.Empty;

    public string SecretKey { get; init; } = string.Empty;
}
