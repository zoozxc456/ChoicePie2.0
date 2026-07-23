using ChoicePie.Backend.Application.Comments.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Comments.Queries;

public sealed record ListCommentsByQuizIdQuery(Guid QuizId) : IRequest<IReadOnlyList<CommentDto>>;
