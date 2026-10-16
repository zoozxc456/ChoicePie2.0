using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Contracts;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.QuizFavorites.Queries;

public sealed class ListMyFavoritesQueryHandler(
    IQuizQueryService quizQueryService,
    ICurrentUserService currentUserService)
    : IRequestHandler<ListMyFavoritesQuery, PagedResult<QuizSummaryDto>>
{
    public Task<PagedResult<QuizSummaryDto>> Handle(ListMyFavoritesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();
        return quizQueryService.ListFavoritesAsync(userId, request.PageNumber, request.PageSize, cancellationToken);
    }
}
