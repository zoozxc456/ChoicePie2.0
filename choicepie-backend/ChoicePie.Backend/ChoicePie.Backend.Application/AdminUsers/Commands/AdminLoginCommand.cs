using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Commands;

public sealed class AdminLoginCommand : IRequest<AdminLoginResultDto>
{
    [Required] [EmailAddress] public required string Email { get; init; }

    [Required] public required string Password { get; init; }
}
