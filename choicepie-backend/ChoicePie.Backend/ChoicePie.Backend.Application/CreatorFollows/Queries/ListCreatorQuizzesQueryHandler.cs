using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.CreatorFollows.Queries;

public sealed class ListCreatorQuizzesQueryHandler(IQuizQueryService quizQueryService)
    : IRequestHandler<ListCreatorQuizzesQuery, PagedResult<QuizSummaryDto>>
{
    public Task<PagedResult<QuizSummaryDto>> Handle(ListCreatorQuizzesQuery request, CancellationToken cancellationToken) =>
        quizQueryService.ListByCreatorAsync(request.CreatorId, request.PageNumber, request.PageSize, cancellationToken);
}
