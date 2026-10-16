using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Application.Account.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Commands;

public sealed class UpdateProfileCommand : IRequest<ProfileDto>
{
    [Required] public required string Name { get; init; }
}
