using System.Text;
using System.Text.RegularExpressions;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;

public sealed record ExtractedPageText(string? Title, string Text);

/// <summary>
/// 以 AngleSharp 把 HTML 轉成純文字：移除非內容元素、優先取 article / main、解碼 entity、整理空白。
/// </summary>
public static partial class WebPageHtmlTextExtractor
{
    private const int PreferredRootMinLength = 200;

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

        return Compose(title, text);
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

    [GeneratedRegex(@"[\s\u200B\uFEFF]+")]
    private static partial Regex InlineWhitespace();
}
