using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Queries;

public sealed class ListMyFollowingQuizzesQueryHandler(
    IQuizQueryService quizQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<ListMyFollowingQuizzesQuery, PagedResult<QuizSummaryDto>>
{
    public Task<PagedResult<QuizSummaryDto>> Handle(ListMyFollowingQuizzesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();
        return quizQueryService.ListFollowingFeedAsync(userId, request.PageNumber, request.PageSize, cancellationToken);
    }
}
