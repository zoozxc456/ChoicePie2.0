using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Shared.Application.Interfaces;

public interface IFileStorageWriter
{
    Task<string> UploadFileAsync(IFormFile file, string destinationPath, CancellationToken ct = default);
    Task DeleteFileAsync(string fileUrl, CancellationToken ct = default);
    Task<string> MoveFileAsync(string sourceFileUrl, string destinationPath, CancellationToken ct = default);
}