using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminMembers.Queries;

public sealed class AdminListMemberQuizzesQuery : PaginationParameters, IRequest<PagedResult<QuizSummaryDto>>
{
    public Guid MemberId { get; set; }
}
