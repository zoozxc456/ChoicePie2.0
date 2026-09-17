using ChoicePie.Backend.Application.Uploads.Commands;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.WebApi.Requests.Uploads;

public sealed record UploadQuizCoverRequest(IFormFile File)
{
    public UploadQuizCoverCommand ToCommand() => new() { File = File };
}
