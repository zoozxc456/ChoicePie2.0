using Amazon.S3;
using Amazon.S3.Model;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.Storage;

public sealed class CloudflareR2FileStorageService : IFileStorageService, IScopedDependency
{
    private readonly AmazonS3Client client;
    private readonly CloudflareR2Settings settings;

    public CloudflareR2FileStorageService(IOptions<CloudflareR2Settings> settings)
    {
        this.settings = settings.Value;

        client = new AmazonS3Client(
            this.settings.AccessKeyId,
            this.settings.SecretAccessKey,
            new AmazonS3Config
            {
                ServiceURL = this.settings.Endpoint,
                ForcePathStyle = true
            });
    }

    public async Task<string> UploadFileAsync(IFormFile file, string destinationPath, CancellationToken ct = default)
    {
        await using var stream = file.OpenReadStream();
        await client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = settings.BucketName,
            Key = destinationPath,
            InputStream = stream,
            ContentType = file.ContentType
        }, ct);

        return $"{settings.PublicBaseUrl.TrimEnd('/')}/{destinationPath}";
    }

    public async Task DeleteFileAsync(string fileUrl, CancellationToken ct = default)
    {
        var key = ExtractObjectNameFromUrl(fileUrl);
        await client.DeleteObjectAsync(settings.BucketName, key, ct);
    }

    public async Task<string> MoveFileAsync(string sourceFileUrl, string destinationPath, CancellationToken ct = default)
    {
        var sourceKey = ExtractObjectNameFromUrl(sourceFileUrl);
        await client.CopyObjectAsync(new CopyObjectRequest
        {
            SourceBucket = settings.BucketName,
            SourceKey = sourceKey,
            DestinationBucket = settings.BucketName,
            DestinationKey = destinationPath
        }, ct);
        await client.DeleteObjectAsync(settings.BucketName, sourceKey, ct);

        return $"{settings.PublicBaseUrl.TrimEnd('/')}/{destinationPath}";
    }

    public async Task<Stream> GetFileAsync(string fileUrl, CancellationToken ct = default)
    {
        var key = ExtractObjectNameFromUrl(fileUrl);
        var response = await client.GetObjectAsync(settings.BucketName, key, ct);
        return response.ResponseStream;
    }

    public async Task<bool> FileExistsAsync(string fileUrl, CancellationToken ct = default)
    {
        var key = ExtractObjectNameFromUrl(fileUrl);
        try
        {
            await client.GetObjectMetadataAsync(settings.BucketName, key, ct);
            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public Task<string> GetSignedUrlAsync(string fileUrl, TimeSpan expiry, CancellationToken ct = default)
    {
        var key = ExtractObjectNameFromUrl(fileUrl);
        var url = client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = settings.BucketName,
            Key = key,
            Expires = DateTime.UtcNow.Add(expiry)
        });

        return Task.FromResult(url);
    }

    public string ExtractObjectNameFromUrl(string signedUrl)
    {
        var prefix = $"{settings.PublicBaseUrl.TrimEnd('/')}/";
        return signedUrl.StartsWith(prefix, StringComparison.Ordinal)
            ? signedUrl[prefix.Length..]
            : new Uri(signedUrl).AbsolutePath.TrimStart('/');
    }
}
