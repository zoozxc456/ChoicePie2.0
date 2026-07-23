using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Commands;

public sealed record AddCreatorFollowCommand(Guid CreatorId) : IRequest;
