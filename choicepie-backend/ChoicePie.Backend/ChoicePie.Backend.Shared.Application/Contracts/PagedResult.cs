namespace ChoicePie.Backend.Shared.Application.Contracts;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount) : BaseResult<T>(Items)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)Math.Max(1, PageSize));

    public bool HasNextPage => PageNumber < TotalPages;
}