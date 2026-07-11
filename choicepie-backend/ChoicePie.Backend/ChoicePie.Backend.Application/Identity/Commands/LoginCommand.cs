using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Identity.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class LoginCommand : IRequest<LoginResultDto>
{
    [Required] [EmailAddress] public required string Email { get; init; }

    [Required] public required string Password { get; init; }
}
