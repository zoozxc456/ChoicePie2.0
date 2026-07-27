using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Commands;

public sealed record RemoveCreatorFollowCommand(Guid CreatorId) : IRequest;
