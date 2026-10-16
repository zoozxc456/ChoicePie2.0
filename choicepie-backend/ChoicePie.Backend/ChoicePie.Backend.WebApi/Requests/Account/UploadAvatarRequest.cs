using ChoicePie.Backend.Application.Account.Commands;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.WebApi.Requests.Account;

public sealed record UploadAvatarRequest(IFormFile File)
{
    public UploadAvatarCommand ToCommand() => new() { File = File };
}
