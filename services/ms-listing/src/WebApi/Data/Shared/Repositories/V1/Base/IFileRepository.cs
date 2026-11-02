namespace WebApi.Data.Shared.Repositories.V1.Base;

public interface IFileRepository
{
    Task UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default);

    // Deleting a file that does not exist succeeds.
    Task DeleteAsync(string key, CancellationToken ct = default);

    // Relative to api-gateway, which serves the files.
    string GetPublicPath(string key);
}
