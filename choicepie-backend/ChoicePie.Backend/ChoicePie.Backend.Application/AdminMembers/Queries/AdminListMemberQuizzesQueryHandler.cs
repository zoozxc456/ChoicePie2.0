using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminListMemberQuizzesQueryHandler(IQuizQueryService quizQueryService)
    : IRequestHandler<AdminListMemberQuizzesQuery, PagedResult<QuizSummaryDto>>
{
    public Task<PagedResult<QuizSummaryDto>> Handle(AdminListMemberQuizzesQuery request, CancellationToken cancellationToken) =>
        quizQueryService.ListAsync(null, null, request.MemberId, request.PageNumber, request.PageSize, cancellationToken);
}
