using System.ComponentModel.DataAnnotations;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Commands;

public sealed class LinkGoogleCommand : IRequest
{
    [Required] public required string IdToken { get; init; }
}
