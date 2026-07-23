using ChoicePie.Backend.Application.Comments.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Comments.Commands;

public sealed record AddCommentCommand(Guid QuizId, string Text) : IRequest<CommentDto>;
