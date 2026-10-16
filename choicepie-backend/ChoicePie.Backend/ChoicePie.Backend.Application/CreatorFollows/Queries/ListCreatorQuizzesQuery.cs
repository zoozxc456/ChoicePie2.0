using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Queries;

public sealed record ListCreatorQuizzesQuery(Guid CreatorId, int PageNumber, int PageSize)
    : IRequest<PagedResult<QuizSummaryDto>>;
