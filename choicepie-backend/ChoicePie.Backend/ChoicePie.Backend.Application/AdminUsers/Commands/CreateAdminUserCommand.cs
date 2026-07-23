using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.AdminUsers.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Commands;

public sealed class CreateAdminUserCommand : IRequest<AdminUserDto>
{
    [Required] [EmailAddress] public required string Email { get; init; }

    [Required] [MinLength(2)] [MaxLength(20)] public required string Name { get; init; }

    [Required] [MinLength(8)] public required string Password { get; init; }

    [Required] [Compare(nameof(Password))] public required string ConfirmPassword { get; init; }

    [Required] public required string Role { get; init; }
}
