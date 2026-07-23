using MediatR;

namespace ChoicePie.Backend.Application.Comments.Commands;

public sealed record DeleteCommentCommand(Guid Id) : IRequest;
