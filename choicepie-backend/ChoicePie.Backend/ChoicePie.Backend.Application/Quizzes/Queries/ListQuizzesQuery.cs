using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class ListQuizzesQuery : PaginationParameters, IRequest<PagedResult<QuizSummaryDto>>
{
    public string? Tag { get; set; }

    public string? Search { get; set; }

    // When set, returns this owner's quizzes across all statuses ("my quizzes"). When unset,
    // only Published quizzes are returned (public listing).
    public Guid? OwnerId { get; set; }
}
