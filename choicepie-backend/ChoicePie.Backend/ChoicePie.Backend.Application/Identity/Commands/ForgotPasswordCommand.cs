using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class ForgotPasswordCommand : IRequest
{
    [Required] [EmailAddress] public required string Email { get; init; }
}
