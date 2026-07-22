using ChoicePie.Backend.Application.Quizzes.Dtos;
using ChoicePie.Backend.Shared.Application.Contracts;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.Application.Quizzes.Queries;

public sealed class ListQuizzesQuery : PaginationParameters, IRequest<PagedResult<QuizSummaryDto>>
{
    [FromQuery(Name = "tag")] public string? Tag { get; set; }

    [FromQuery(Name = "search")] public string? Search { get; set; }

    // When set, returns this owner's quizzes across all statuses ("my quizzes"). When unset,
    // only Published quizzes are returned (public listing).
    [FromQuery(Name = "ownerId")] public Guid? OwnerId { get; set; }
}
