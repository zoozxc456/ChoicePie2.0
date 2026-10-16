using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

public sealed record QuizCover : ValueObject
{
    // 6 個固定色鍵，對應前端 cp-* 語意色（primary/secondary/success/danger/warning/info）。
    // 純展示資料、不參與業務分支，用簡單 allow-list 驗證即可，不需要 Enumeration<T> 那套機制。
    private static readonly string[] AllowedColorKeys =
        ["primary", "secondary", "success", "danger", "warning", "info"];

    public string? ImageUrl { get; }
    public string Emoji { get; }
    public string Gradient { get; }

    private QuizCover(string? imageUrl, string emoji, string gradient)
    {
        ImageUrl = imageUrl;
        Emoji = emoji;
        Gradient = gradient;
    }

    public static QuizCover Create(string? imageUrl, string emoji, string gradient)
    {
        if (string.IsNullOrWhiteSpace(emoji))
        {
            throw new InvalidQuizException("封面圖示不能為空。");
        }

        if (!AllowedColorKeys.Contains(gradient))
        {
            throw new InvalidQuizException($"未知的封面顏色：{gradient}");
        }

        if (imageUrl is { Length: > 2048 })
        {
            throw new InvalidQuizException("封面圖片網址過長。");
        }

        return new QuizCover(imageUrl, emoji, gradient);
    }
}
