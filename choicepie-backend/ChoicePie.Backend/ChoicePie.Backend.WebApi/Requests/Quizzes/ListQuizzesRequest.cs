using ChoicePie.Backend.Application.Quizzes.Queries;
using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Requests.Quizzes;

public sealed class ListQuizzesRequest
{
    [FromQuery(Name = "tag")] public string? Tag { get; set; }

    [FromQuery(Name = "search")] public string? Search { get; set; }

    [FromQuery(Name = "mine")] public bool Mine { get; set; }

    [FromQuery(Name = "page")] public int PageNumber { get; set; } = 1;

    [FromQuery(Name = "pageSize")] public int PageSize { get; set; } = 20;

    public ListQuizzesQuery ToQuery(Guid? ownerId) => new()
    {
        Tag = Tag, Search = Search, OwnerId = ownerId, PageNumber = PageNumber, PageSize = PageSize
    };
}
