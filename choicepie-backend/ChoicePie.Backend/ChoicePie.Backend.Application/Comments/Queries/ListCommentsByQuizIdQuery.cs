using ChoicePie.Backend.Application.Comments.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.Comments.Queries;

public sealed record ListCommentsByQuizIdQuery(Guid QuizId, int PageNumber, int PageSize)
    : IRequest<PagedResult<CommentDto>>;
