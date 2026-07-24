using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizzes.Queries;

public sealed class AdminListQuizzesQueryHandler(IQuizQueryService quizQueryService)
    : IRequestHandler<AdminListQuizzesQuery, PagedResult<QuizSummaryDto>>
{
    public Task<PagedResult<QuizSummaryDto>> Handle(AdminListQuizzesQuery request, CancellationToken cancellationToken) =>
        quizQueryService.AdminListAsync(request.Search, request.PageNumber, request.PageSize, cancellationToken);
}
