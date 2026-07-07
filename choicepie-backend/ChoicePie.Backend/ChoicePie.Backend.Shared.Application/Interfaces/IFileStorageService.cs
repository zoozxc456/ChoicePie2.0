using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string destinationPath, CancellationToken ct = default);
    Task DeleteFileAsync(string fileUrl, CancellationToken ct = default);
    Task<string> MoveFileAsync(string sourceFileUrl, string destinationPath, CancellationToken ct = default);
    Task<Stream> GetFileAsync(string fileUrl, CancellationToken ct = default);
    Task<bool> FileExistsAsync(string fileUrl, CancellationToken ct = default);
    Task<string> GetSignedUrlAsync(string fileUrl, TimeSpan expiry, CancellationToken ct = default);
    string ExtractObjectNameFromUrl(string signedUrl);
}