using Amazon.Runtime;
using Amazon.S3;
using WebApi.Data.Shared.Models.V1;

namespace WebApi.Extensions.V1;

public static class StorageExtensions
{
    // MinIO ignores the region, but request signing needs one.
    private const string SigningRegion = "us-east-1";
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(10);

    public static void InitStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(StorageOptions.SectionName);
        var storage = section.Get<StorageOptions>()
            ?? throw new InvalidOperationException($"The '{StorageOptions.SectionName}' section is missing from appsettings.");

        // The keys come from Vault (Storage:AccessKey, Storage:SecretKey); appsettings has the endpoint and bucket.
        if (string.IsNullOrWhiteSpace(storage.AccessKey) || string.IsNullOrWhiteSpace(storage.SecretKey))
        {
            throw new InvalidOperationException(
                $"'{StorageOptions.SectionName}:{nameof(StorageOptions.AccessKey)}' and '{StorageOptions.SectionName}:{nameof(StorageOptions.SecretKey)}' are missing; they are read from Vault.");
        }

        services.Configure<StorageOptions>(section);
        services.AddSingleton<IAmazonS3>(_ => new AmazonS3Client(
            new BasicAWSCredentials(storage.AccessKey, storage.SecretKey),
            new AmazonS3Config
            {
                ServiceURL = storage.Endpoint,
                ForcePathStyle = true,
                AuthenticationRegion = SigningRegion,
                Timeout = RequestTimeout,
                MaxErrorRetry = 1
            }));
    }
}
