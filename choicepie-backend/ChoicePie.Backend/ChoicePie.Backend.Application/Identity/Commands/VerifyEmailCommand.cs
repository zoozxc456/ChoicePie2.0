using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class VerifyEmailCommand : IRequest
{
    [Required] public required string Token { get; init; }
}
