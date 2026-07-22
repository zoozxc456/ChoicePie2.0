using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Identity.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class RefreshTokenCommand : IRequest<LoginResultDto>
{
    [Required] public required string RefreshToken { get; init; }
}
