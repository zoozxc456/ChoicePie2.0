using Microsoft.AspNetCore.Mvc;

namespace ChoicePie.Backend.Shared.Application.Contracts;

public abstract class PaginationParameters
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;

    /// <summary>
    /// 當前頁碼，從 1 開始。
    /// </summary>

    [FromQuery(Name = "page")]
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// 每頁顯示的資料筆數。
    /// </summary>
    [FromQuery(Name = "pageSize")]
    public int PageSize
    {
        get => _pageSize;
        set
        {
            _pageSize = value switch
            {
                <= 0 => _pageSize,
                > MaxPageSize => MaxPageSize,
                _ => value
            };
        }
    }
}