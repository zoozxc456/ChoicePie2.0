using ChoicePie.Backend.Application.Comments.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Comments.Commands;

public sealed record UpdateCommentCommand(Guid Id, string Text) : IRequest<CommentDto>;
