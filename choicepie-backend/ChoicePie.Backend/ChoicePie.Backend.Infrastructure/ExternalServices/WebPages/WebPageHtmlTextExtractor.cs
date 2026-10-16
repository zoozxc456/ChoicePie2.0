using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using ChoicePie.Backend.Application.Quizzes;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;

public sealed record ExtractedPageText(string? Title, string Text, bool LooksLikeAppShell = false);

/// <summary>
/// 以 AngleSharp 把 HTML 轉成純文字：移除非內容元素、優先取 article / main、解碼 entity、整理空白。
/// </summary>
public static partial class WebPageHtmlTextExtractor
{
    private const int PreferredRootMinLength = 200;

    /// <summary>主要內文低於此長度（與 QuizContentInput.MinContentLength 一致）時改用結構化資料後備。</summary>
    private const int MinMainTextLength = QuizContentInput.MinContentLength;

    public const int MaxFallbackLength = WebPageStructuredDataExtractor.MaxTotalLength;

    private static readonly string[] AppRootSelectors = ["#__next", "#__nuxt", "#app", "#root"];

    private const string NonContentSelector =
        "script,style,noscript,template,svg,nav,header,footer,aside,form,iframe,object,embed,canvas,[hidden]";

    private static readonly HashSet<string> BlockElements = new(StringComparer.OrdinalIgnoreCase)
    {
        "address", "article", "blockquote", "br", "dd", "details", "div", "dl", "dt", "fieldset", "figcaption",
        "figure", "h1", "h2", "h3", "h4", "h5", "h6", "hr", "li", "main", "ol", "p", "pre", "section", "summary",
        "table", "tbody", "td", "tfoot", "th", "thead", "tr", "ul"
    };

    public static ExtractedPageText ExtractFromHtml(string html)
    {
        var document = new HtmlParser().ParseDocument(html);

        var title = Normalize(document.Title ?? string.Empty);

        // 結構化資料與 app shell 判斷必須在移除 script / noscript 之前取得。
        var structuredText = WebPageStructuredDataExtractor.Extract(document);
        var noscriptAsksForJs = document.QuerySelectorAll("noscript")
            .Any(n => JavaScriptNotice().IsMatch(n.TextContent));
        var appRoots = AppRootSelectors.Select(selector => document.QuerySelector(selector)).OfType<IElement>().ToList();

        foreach (var element in document.QuerySelectorAll(NonContentSelector).ToList())
        {
            element.Remove();
        }

        var body = document.Body ?? document.DocumentElement;
        var bodyText = body is null ? string.Empty : ExtractText(body);
        var preferred = PickPreferredRootText(document);
        // 內文容器太短（例如只是卡片摘要）時，退回整個 body，避免漏掉主要內容。
        var text = preferred is not null
                   && (preferred.Length >= PreferredRootMinLength || preferred.Length >= bodyText.Length)
            ? preferred
            : bodyText;

        if (text.Length < MinMainTextLength && structuredText.Length > 0)
        {
            text = text.Length == 0 ? structuredText : $"{text}\n{structuredText}";
        }

        var looksLikeAppShell = noscriptAsksForJs
                                || appRoots.Any(root => Normalize(root.TextContent).Length < MinMainTextLength);

        return Compose(title, text) with { LooksLikeAppShell = looksLikeAppShell };
    }

    public static ExtractedPageText ExtractFromPlainText(string text) => new(null, Normalize(text));

    private static string? PickPreferredRootText(IDocument document)
    {
        return document.QuerySelectorAll("article, main")
            .Select(ExtractText)
            .OrderByDescending(t => t.Length)
            .FirstOrDefault();
    }

    private static ExtractedPageText Compose(string title, string text)
    {
        var finalTitle = string.IsNullOrWhiteSpace(title) ? null : title;
        var combined = finalTitle is null ? text : string.IsNullOrEmpty(text) ? finalTitle : $"{finalTitle}\n\n{text}";
        return new ExtractedPageText(finalTitle, combined);
    }

    private static string ExtractText(IElement root)
    {
        var builder = new StringBuilder();
        Append(root, builder);
        return Normalize(builder.ToString());
    }

    private static void Append(INode node, StringBuilder builder)
    {
        switch (node)
        {
            case IText text:
                builder.Append(text.Data);
                break;
            case IElement element:
                var isBlock = BlockElements.Contains(element.LocalName);
                if (isBlock) builder.Append('\n');
                foreach (var child in element.ChildNodes)
                {
                    Append(child, builder);
                }

                if (isBlock) builder.Append('\n');
                break;
        }
    }

    /// <summary>每行內連續空白收斂成單一空白、去除行首尾空白、丟棄空行（區塊元素各自成行）。</summary>
    private static string Normalize(string text)
    {
        var lines = text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n')
            .Select(line => InlineWhitespace().Replace(line, " ").Trim())
            .Where(line => line.Length > 0);

        return string.Join('\n', lines);
    }

    [GeneratedRegex(@"enable\s+javascript|javascript\s+(is\s+)?(required|disabled|must)|requires?\s+javascript|啟用\s*javascript|開啟\s*javascript|需要\s*javascript|JavaScript\s*(已停用|被停用)", RegexOptions.IgnoreCase)]
    private static partial Regex JavaScriptNotice();

    [GeneratedRegex(@"[\s\u200B\uFEFF]+")]
    private static partial Regex InlineWhitespace();
}
