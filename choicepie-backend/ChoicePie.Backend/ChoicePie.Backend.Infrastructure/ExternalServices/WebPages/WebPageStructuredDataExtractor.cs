using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using AngleSharp.Dom;

namespace ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;

/// <summary>
/// 主要內文不足時的後備來源：直接讀 HTML 內已有的結構化資料（JSON-LD、meta / OpenGraph、
/// Next / Nuxt 內嵌狀態），不執行任何 JavaScript。所有解析皆容錯，格式錯誤時回傳空結果而不丟例外。
/// </summary>
internal static partial class WebPageStructuredDataExtractor
{
    public const int MaxTotalLength = 20_000;

    private const int MaxJsonDepth = 64;
    private const int MinCjkLength = 8;
    private const int MinLatinLength = 20;

    private static readonly string[] JsonLdKeys = ["headline", "name", "description", "articleBody"];

    // 狀態 JSON 中明顯是技術欄位的鍵，其字串值不採用。
    private static readonly HashSet<string> SkippedStateKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "_id", "uid", "uuid", "key", "slug", "path", "url", "href", "src", "srcset", "image", "img", "icon",
        "class", "classname", "style", "css", "type", "locale", "lang", "buildid", "assetprefix", "runtimeconfig",
        "page", "query", "token", "csrf", "hash", "sha", "etag", "script", "scripts", "chunks", "component"
    };

    public static string Extract(IDocument document)
    {
        var collector = new Collector(MaxTotalLength);

        foreach (var value in ReadJsonLd(document)) collector.Add(value);
        foreach (var value in ReadMeta(document)) collector.Add(value);
        foreach (var value in ReadEmbeddedState(document)) collector.Add(value, requireReadable: true);

        return collector.ToText();
    }

    private static IEnumerable<string> ReadJsonLd(IDocument document)
    {
        foreach (var script in document.QuerySelectorAll("script"))
        {
            var type = script.GetAttribute("type");
            if (type is null || !type.Trim().Equals("application/ld+json", StringComparison.OrdinalIgnoreCase)) continue;

            foreach (var value in ParseJsonLd(script.TextContent))
            {
                yield return value;
            }
        }
    }

    private static List<string> ParseJsonLd(string json)
    {
        var result = new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = MaxJsonDepth, AllowTrailingCommas = true });
            CollectJsonLd(doc.RootElement, result, depth: 0);
        }
        catch (JsonException)
        {
            // 格式錯誤的 JSON-LD 直接忽略。
        }

        return result;
    }

    private static void CollectJsonLd(JsonElement element, List<string> result, int depth)
    {
        if (depth > 3) return;

        switch (element.ValueKind)
        {
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray()) CollectJsonLd(item, result, depth + 1);
                break;
            case JsonValueKind.Object:
                foreach (var key in JsonLdKeys)
                {
                    if (element.TryGetProperty(key, out var value) && value.ValueKind == JsonValueKind.String)
                    {
                        result.Add(StripHtml(value.GetString()!));
                    }
                }

                if (element.TryGetProperty("@graph", out var graph)) CollectJsonLd(graph, result, depth + 1);
                break;
        }
    }

    private static IEnumerable<string> ReadMeta(IDocument document)
    {
        string[] wanted = ["description", "og:title", "og:description", "twitter:description"];
        var found = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var meta in document.QuerySelectorAll("meta"))
        {
            var key = (meta.GetAttribute("name") ?? meta.GetAttribute("property"))?.Trim();
            var content = meta.GetAttribute("content");
            if (key is null || string.IsNullOrWhiteSpace(content)) continue;
            if (wanted.Contains(key, StringComparer.OrdinalIgnoreCase)) found.TryAdd(key, content);
        }

        foreach (var key in wanted)
        {
            if (found.TryGetValue(key, out var value)) yield return StripHtml(value);
        }
    }

    private static IEnumerable<string> ReadEmbeddedState(IDocument document)
    {
        foreach (var script in document.QuerySelectorAll("script"))
        {
            var id = script.Id;
            var text = script.TextContent;
            if (string.IsNullOrWhiteSpace(text)) continue;

            if (string.Equals(id, "__NEXT_DATA__", StringComparison.Ordinal)
                || string.Equals(id, "__NUXT_DATA__", StringComparison.Ordinal))
            {
                foreach (var value in HarvestJson(text)) yield return value;
            }
            else if (WindowNuxt().IsMatch(text))
            {
                foreach (var value in HarvestNuxtAssignment(text)) yield return value;
            }
        }
    }

    private static List<string> HarvestJson(string json)
    {
        var result = new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = MaxJsonDepth, AllowTrailingCommas = true });
            WalkState(doc.RootElement, result, key: null);
        }
        catch (JsonException)
        {
        }

        return result;
    }

    private static void WalkState(JsonElement element, List<string> result, string? key)
    {
        if (result.Count > 5000) return;

        switch (element.ValueKind)
        {
            case JsonValueKind.String:
                if (key is null || !SkippedStateKeys.Contains(key)) result.Add(element.GetString()!);
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray()) WalkState(item, result, key);
                break;
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject()) WalkState(property.Value, result, property.Name);
                break;
        }
    }

    /// <summary>window.__NUXT__ 可能是純 JSON，也可能是 (function(a){return {...}})(args) 的 JS；後者退而掃描字串字面值。</summary>
    private static List<string> HarvestNuxtAssignment(string script)
    {
        var start = script.IndexOf('{');
        var end = script.LastIndexOf('}');
        if (start >= 0 && end > start)
        {
            var candidate = script[start..(end + 1)];
            var parsed = HarvestJson(candidate);
            if (parsed.Count > 0) return parsed;
        }

        var result = new List<string>();
        foreach (Match match in StringLiteral().Matches(script))
        {
            try
            {
                result.Add(JsonSerializer.Deserialize<string>(match.Value) ?? string.Empty);
            }
            catch (JsonException)
            {
            }

            if (result.Count > 5000) break;
        }

        return result;
    }

    internal static string StripHtml(string value)
    {
        var text = value.Contains('<') ? HtmlTag().Replace(value, "\n") : value;
        return WebUtility.HtmlDecode(text);
    }

    /// <summary>是否為「人類可讀」文字：排除網址、ID、雜湊、程式碼 / CSS 與過短的字串。</summary>
    internal static bool IsReadable(string value)
    {
        var s = value.Trim();
        if (s.Length < MinCjkLength) return false;

        if (s.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || s.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || s.StartsWith("//", StringComparison.Ordinal)
            || s.StartsWith('/')
            || s.StartsWith("data:", StringComparison.OrdinalIgnoreCase)
            || s.StartsWith("#", StringComparison.Ordinal))
        {
            return false;
        }

        if (!s.Any(char.IsWhiteSpace) && TokenLike().IsMatch(s)) return false; // ID / hash / base64 / slug
        if (CodeLike().IsMatch(s)) return false;

        var letters = s.Count(c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || IsCjkPunctuation(c) || ".,!?;:'\"-()".Contains(c));
        if ((double)letters / s.Length < 0.85) return false;

        var hasCjk = s.Any(c => c >= 0x2E80);
        if (hasCjk) return true;

        return s.Length >= MinLatinLength && s.Count(char.IsWhiteSpace) >= 2;
    }

    private static bool IsCjkPunctuation(char c) => c is >= '　' and <= '〿' or >= '＀' and <= '￯';

    [GeneratedRegex(@"^\s*window\s*\.\s*__NUXT__\s*=")]
    private static partial Regex WindowNuxt();

    [GeneratedRegex("\"(?:[^\"\\\\\\r\\n]|\\\\.)*\"")]
    private static partial Regex StringLiteral();

    [GeneratedRegex("<[^>]+>")]
    private static partial Regex HtmlTag();

    [GeneratedRegex(@"^[A-Za-z0-9+/=_\-:.@]{8,}$")]
    private static partial Regex TokenLike();

    [GeneratedRegex(@"[{};]\s*[\w\-.#]*\s*[:{]|=>|\bfunction\s*\(|\bvar\s+\w+\s*=|</?\w+[^>]*>|\$\{|\bconst\s+\w+\s*=")]
    private static partial Regex CodeLike();

    private sealed class Collector(int maxLength)
    {
        private readonly List<string> items = [];
        private int length;

        public void Add(string raw, bool requireReadable = false)
        {
            foreach (var piece in raw.Replace("\r\n", "\n").Split('\n'))
            {
                var value = Whitespace().Replace(piece, " ").Trim();
                if (value.Length == 0 || length >= maxLength) continue;
                if (requireReadable && !IsReadable(value)) continue;
                if (items.Any(existing => existing.Contains(value, StringComparison.Ordinal))) continue;

                if (length + value.Length > maxLength) value = value[..(maxLength - length)];
                items.Add(value);
                length += value.Length + 1;
            }
        }

        public string ToText() => string.Join('\n', items);

        private static Regex Whitespace() => WhitespaceRegex();
    }

    [GeneratedRegex(@"[\s​﻿]+")]
    private static partial Regex WhitespaceRegex();
}
