namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface IFileStorageReader
{
    Task<Stream> GetFileAsync(string fileUrl, CancellationToken ct = default);
    Task<bool> FileExistsAsync(string fileUrl, CancellationToken ct = default);
    Task<string> GetSignedUrlAsync(string fileUrl, TimeSpan expiry, CancellationToken ct = default);
    string ExtractObjectNameFromUrl(string signedUrl);
}