using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Commands;

public sealed class AdminRefreshTokenCommand : IRequest<AdminLoginResultDto>
{
    [Required] public required string RefreshToken { get; init; }
}
