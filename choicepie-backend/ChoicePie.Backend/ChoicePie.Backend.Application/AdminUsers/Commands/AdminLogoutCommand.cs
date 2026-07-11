using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Commands;

public sealed class AdminLogoutCommand : IRequest
{
    [Required] public required string RefreshToken { get; init; }
}
