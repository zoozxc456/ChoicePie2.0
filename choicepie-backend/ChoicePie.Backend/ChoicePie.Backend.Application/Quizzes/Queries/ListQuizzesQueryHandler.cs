using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class ListQuizzesQueryHandler(IQuizQueryService quizQueryService)
    : IRequestHandler<ListQuizzesQuery, PagedResult<QuizSummaryDto>>
{
    public Task<PagedResult<QuizSummaryDto>> Handle(ListQuizzesQuery request, CancellationToken cancellationToken) =>
        quizQueryService.ListAsync(
            request.Tag, request.Search, request.OwnerId, request.PageNumber, request.PageSize, cancellationToken);
}
