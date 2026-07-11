using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class LogoutCommand : IRequest
{
    [Required] public required string RefreshToken { get; init; }
}
