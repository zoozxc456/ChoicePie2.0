using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;

namespace ChoicePie.Backend.Application.Quizzes;

/// <summary>
/// 判斷使用者輸入的出題內容是「網址」還是「純文字」。
/// 規則：去除前後空白後，不含任何空白且含有 "://" 的單一字串視為網址；其餘一律視為純文字。
/// </summary>
public static class QuizContentInput
{
    public const int MinContentLength = 30;
    public const int MaxContentLength = 5000;

    public static bool LooksLikeUrl(string? input)
    {
        var trimmed = input?.Trim();
        return !string.IsNullOrEmpty(trimmed)
               && trimmed.Contains("://", StringComparison.Ordinal)
               && !trimmed.Any(char.IsWhiteSpace);
    }

    /// <summary>
    /// 輸入為網址時回傳解析後的 http/https 網址；為純文字時回傳 null。
    /// </summary>
    /// <exception cref="UnsupportedWebPageUrlException">看起來像網址，但不是合法的絕對 http/https 網址。</exception>
    public static Uri? ParseUrlOrNull(string? input)
    {
        if (!LooksLikeUrl(input)) return null;

        if (!Uri.TryCreate(input!.Trim(), UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new UnsupportedWebPageUrlException("Input is not an absolute http/https URL.");
        }

        return uri;
    }
}
