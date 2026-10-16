using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Account.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Application.Account.Commands;

public sealed class UploadAvatarCommand : IRequest<ProfileDto>
{
    [Required] public required IFormFile File { get; init; }
}
