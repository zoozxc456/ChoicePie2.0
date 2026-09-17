using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Uploads.Dtos;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ChoicePie.Backend.Application.Uploads.Commands;

public sealed class UploadQuizCoverCommand : IRequest<UploadQuizCoverResultDto>
{
    [Required] public required IFormFile File { get; init; }
}
