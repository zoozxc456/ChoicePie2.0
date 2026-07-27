using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.AdminQuizzes.Queries;

public sealed class AdminListQuizzesQuery : PaginationParameters, IRequest<PagedResult<QuizSummaryDto>>
{
    public string? Search { get; set; }
}
