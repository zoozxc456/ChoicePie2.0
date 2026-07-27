using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class ResetPasswordCommand : IRequest
{
    [Required] public required string Token { get; init; }

    [Required] [MinLength(8)] public required string Password { get; init; }

    [Required] [Compare(nameof(Password))] public required string ConfirmPassword { get; init; }
}
