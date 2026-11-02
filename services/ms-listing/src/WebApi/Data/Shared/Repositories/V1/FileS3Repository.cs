using System.Net;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using WebApi.Data.Shared.Models.V1;
using WebApi.Data.Shared.Repositories.V1.Base;
using WebApi.Exceptions.V1;

namespace WebApi.Data.Shared.Repositories.V1;

public sealed class FileS3Repository : IFileRepository
{
    // Short enough for a deleted image to leave browsers soon; ETag keeps revalidation cheap.
    private const string CacheControl = "public, max-age=86400";

    private readonly IAmazonS3 client;
    private readonly StorageOptions options;

    public FileS3Repository(IAmazonS3 client, IOptions<StorageOptions> options)
    {
        this.client = client;
        this.options = options.Value;
    }

    public Task UploadAsync(string key, Stream content, string contentType, CancellationToken ct = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = this.options.Bucket,
            Key = key,
            InputStream = content,
            AutoCloseStream = false,
            ContentType = contentType
        };
        request.Headers.CacheControl = CacheControl;

        return this.SendAsync(() => this.client.PutObjectAsync(request, ct), ct);
    }

    public Task DeleteAsync(string key, CancellationToken ct = default)
    {
        return this.SendAsync(() => this.client.DeleteObjectAsync(this.options.Bucket, key, ct), ct);
    }

    public string GetPublicPath(string key)
    {
        return $"/{this.options.Bucket}/{key}";
    }

    // Storage being down is reported as 503; anything else, such as a wrong key, stays a server error.
    private async Task SendAsync(Func<Task> send, CancellationToken ct)
    {
        try
        {
            await send();
        }
        catch (AmazonServiceException exception) when (exception.StatusCode >= HttpStatusCode.InternalServerError || exception.StatusCode == 0)
        {
            throw new StorageUnavailableException("File storage is not available.", exception);
        }
        catch (HttpRequestException exception)
        {
            throw new StorageUnavailableException("File storage is not available.", exception);
        }
        catch (TaskCanceledException exception) when (!ct.IsCancellationRequested)
        {
            throw new StorageUnavailableException("File storage is not available.", exception);
        }
    }
}
