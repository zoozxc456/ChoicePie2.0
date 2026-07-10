using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Identity.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class RegisterMemberCommand : IRequest<MemberDto>
{
    [Required] [EmailAddress] public required string Email { get; init; }

    [Required] [MinLength(2)] [MaxLength(20)] public required string Name { get; init; }

    [Required] [MinLength(8)] public required string Password { get; init; }

    [Required] [Compare(nameof(Password))] public required string ConfirmPassword { get; init; }
}
