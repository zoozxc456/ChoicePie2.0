using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Identity.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Identity.Commands;

public sealed class GoogleLoginCommand : IRequest<LoginResultDto>
{
    [Required] public required string IdToken { get; init; }
}
