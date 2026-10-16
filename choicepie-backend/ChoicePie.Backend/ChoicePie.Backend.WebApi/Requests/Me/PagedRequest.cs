using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.WebApi.Requests.Me;

public sealed class PagedRequest
{
    private const int MaxPageSize = 100;

    [FromQuery(Name = "page")] public int PageNumber { get; set; } = 1;

    [FromQuery(Name = "pageSize")] public int PageSize { get; set; } = 20;

    public int NormalizedPageNumber => Math.Max(1, PageNumber);

    public int NormalizedPageSize => PageSize <= 0 ? 20 : Math.Min(PageSize, MaxPageSize);
}
