using ChoicePie.Backend.Infrastructure.ExternalServices.WebPages;

namespace ChoicePie.Backend.Infrastructure.Tests.ExternalServices.WebPages;

[TestFixture]
public class WebPageHtmlTextExtractorTests
{
    private static readonly string LongParagraph = string.Join(" ", Enumerable.Repeat("這是一段足夠長的文章內容。", 30));

    [Test]
    public void ExtractFromHtml_GivenNonContentElements_WhenExtracted_ThenDropsThem()
    {
        const string html = """
            <html><head><title>T</title><style>.x{color:red}</style><script>var secret=1;</script></head>
            <body>
              <header>SITE-HEADER</header><nav>SITE-NAV</nav>
              <p>Body paragraph</p>
              <script>alert('x')</script><noscript>NOSCRIPT</noscript><template><p>TEMPLATE</p></template>
              <svg><text>SVG-TEXT</text></svg><form><label>FORM-LABEL</label></form>
              <aside>SIDEBAR</aside><footer>SITE-FOOTER</footer>
            </body></html>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text, Is.EqualTo("T\n\nBody paragraph"));
    }

    [Test]
    public void ExtractFromHtml_GivenArticleWithEnoughText_WhenExtracted_ThenPrefersArticleOverSurroundingBody()
    {
        var html = $"<body><div>COMMENTS-AND-RELATED</div><article><h1>Heading</h1><p>{LongParagraph}</p></article><div>MORE-NOISE</div></body>";

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.Multiple(() =>
        {
            Assert.That(result.Text, Does.Contain("Heading").And.Contain("這是一段足夠長的文章內容。"));
            Assert.That(result.Text, Does.Not.Contain("COMMENTS-AND-RELATED").And.Not.Contain("MORE-NOISE"));
        });
    }

    [Test]
    public void ExtractFromHtml_GivenMainWithoutArticle_WhenExtracted_ThenPrefersMain()
    {
        var html = $"<body><div>NOISE</div><main><p>{LongParagraph}</p></main></body>";

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text, Does.Not.Contain("NOISE"));
    }

    [Test]
    public void ExtractFromHtml_GivenTinyArticleButLargeBody_WhenExtracted_ThenFallsBackToBody()
    {
        var html = $"<body><article>teaser</article><div><p>{LongParagraph}</p></div></body>";

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text, Does.Contain("這是一段足夠長的文章內容。"));
    }

    [Test]
    public void ExtractFromHtml_GivenTitle_WhenExtracted_ThenReturnsTitleAndPrependsItToText()
    {
        var result = WebPageHtmlTextExtractor.ExtractFromHtml("<html><head><title>  My   Page </title></head><body><p>Hello</p></body></html>");

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.EqualTo("My Page"));
            Assert.That(result.Text, Is.EqualTo("My Page\n\nHello"));
        });
    }

    [Test]
    public void ExtractFromHtml_GivenNoTitle_WhenExtracted_ThenTitleIsNull()
    {
        var result = WebPageHtmlTextExtractor.ExtractFromHtml("<body><p>Hello</p></body>");

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.Null);
            Assert.That(result.Text, Is.EqualTo("Hello"));
        });
    }

    [Test]
    public void ExtractFromHtml_GivenEntitiesAndMessyWhitespace_WhenExtracted_ThenDecodesAndCollapses()
    {
        const string html = "<body><p>Tom &amp; Jerry &lt;3&nbsp;&nbsp;&#x4E2D;&#25991;</p>\n\n\n<p>   many    spaces\tand\ttabs  </p></body>";

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text, Is.EqualTo("Tom & Jerry <3 中文\nmany spaces and tabs"));
    }

    [Test]
    public void ExtractFromHtml_GivenAdjacentBlocksAndBr_WhenExtracted_ThenSeparatesThemWithLineBreaks()
    {
        var result = WebPageHtmlTextExtractor.ExtractFromHtml("<body><p>one</p><p>two<br>three</p><ul><li>a</li><li>b</li></ul>x<span>y</span><span>z</span></body>");

        Assert.That(result.Text, Is.EqualTo("one\ntwo\nthree\na\nb\nxyz"));
    }

    [Test]
    public void ExtractFromHtml_GivenHiddenElement_WhenExtracted_ThenDropsIt()
    {
        var result = WebPageHtmlTextExtractor.ExtractFromHtml("<body><p>shown</p><p hidden>secret</p></body>");

        Assert.That(result.Text, Is.EqualTo("shown"));
    }

    [Test]
    public void ExtractFromPlainText_GivenMessyText_WhenExtracted_ThenNormalizesWhitespace()
    {
        var result = WebPageHtmlTextExtractor.ExtractFromPlainText("  line   one \r\n\r\n\r\n line\ttwo  ");

        Assert.Multiple(() =>
        {
            Assert.That(result.Title, Is.Null);
            Assert.That(result.Text, Is.EqualTo("line one\nline two"));
        });
    }

    // ---- 結構化資料後備（無 JavaScript 執行） ----

    private const string LongBody = "這是從結構化資料取得的文章內容，長度足夠通過最小字數限制，用來驗證後備擷取邏輯。";

    [Test]
    public void ExtractFromHtml_GivenEmptyBodyWithJsonLd_WhenExtracted_ThenUsesArticleBodyAndHeadline()
    {
        var html = $$$"""
            <html><head><title>站名</title>
            <script type="application/ld+json">{"@type":"Article","headline":"JSON-LD 標題","articleBody":"{{{LongBody}}}"}</script>
            </head><body><div id="root"></div></body></html>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.Multiple(() =>
        {
            Assert.That(result.Text, Does.Contain("JSON-LD 標題").And.Contain(LongBody));
            Assert.That(result.Text, Does.StartWith("站名"));
        });
    }

    [Test]
    public void ExtractFromHtml_GivenJsonLdGraph_WhenExtracted_ThenReadsGraphItemsAndStripsHtml()
    {
        var html = $$$"""
            <head><script type="application/ld+json">
            {"@context":"https://schema.org","@graph":[
              {"@type":"WebSite","name":"我的網站"},
              {"@type":"Article","description":"圖表內的描述","articleBody":"<p>{{{LongBody}}}</p>"}
            ]}</script></head><body></body>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.Multiple(() =>
        {
            Assert.That(result.Text, Does.Contain("我的網站").And.Contain("圖表內的描述").And.Contain(LongBody));
            Assert.That(result.Text, Does.Not.Contain("<p>"));
        });
    }

    [Test]
    public void ExtractFromHtml_GivenJsonLdArray_WhenExtracted_ThenReadsEachItem()
    {
        var html = $$$"""
            <head><script type="application/ld+json">[{"headline":"第一"},{"description":"{{{LongBody}}}"}]</script></head><body></body>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text, Does.Contain("第一").And.Contain(LongBody));
    }

    [Test]
    public void ExtractFromHtml_GivenOnlyMetaAndOpenGraph_WhenExtracted_ThenCombinesThemWithoutDuplicates()
    {
        var html = $$$"""
            <head><title>T</title>
            <meta name="description" content="{{{LongBody}}}">
            <meta property="og:title" content="OG 標題">
            <meta property="og:description" content="{{{LongBody}}}">
            <meta name="twitter:description" content="Twitter 描述內容">
            </head><body></body>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.Multiple(() =>
        {
            Assert.That(result.Text, Does.Contain("OG 標題").And.Contain("Twitter 描述內容"));
            Assert.That(result.Text.Split(LongBody).Length - 1, Is.EqualTo(1));
        });
    }

    [Test]
    public void ExtractFromHtml_GivenNextData_WhenExtracted_ThenHarvestsReadableStringsOnly()
    {
        var html = $$$"""
            <body><div id="__next"></div>
            <script id="__NEXT_DATA__" type="application/json">
            {"props":{"pageProps":{"post":{"title":"Next 文章標題","body":"{{{LongBody}}}","url":"https://example.com/a/very/long/path/to/page","id":"3f2504e0-4f89-11d3-9a0c-0305e82c3301","hash":"a94a8fe5ccb19ba61c4c0873d391e987982fbbd3","css":".a{color:red;margin:0}","short":"ok"} } },"buildId":"abcDEF123456789xyz"}
            </script></body>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.Multiple(() =>
        {
            Assert.That(result.Text, Does.Contain("Next 文章標題").And.Contain(LongBody));
            Assert.That(result.Text, Does.Not.Contain("https://example.com").And.Not.Contain("3f2504e0")
                .And.Not.Contain("a94a8fe5").And.Not.Contain("color:red").And.Not.Contain("buildId"));
        });
    }

    [Test]
    public void ExtractFromHtml_GivenNuxtDataArray_WhenExtracted_ThenHarvestsStrings()
    {
        var html = $$$"""
            <body><div id="__nuxt"></div>
            <script type="application/json" id="__NUXT_DATA__">[["ShallowReactive",1],{"data":2},{"post":3},{"title":4,"body":5},"Nuxt 文章標題","{{{LongBody}}}"]</script></body>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text, Does.Contain("Nuxt 文章標題").And.Contain(LongBody)
            .And.Not.Contain("ShallowReactive"));
    }

    [Test]
    public void ExtractFromHtml_GivenWindowNuxtAssignment_WhenExtracted_ThenHarvestsStringLiterals()
    {
        var html = $$$"""
            <body><div id="__nuxt"></div>
            <script>window.__NUXT__={"data":[{"title":"舊版 Nuxt 標題","content":"{{{LongBody}}}"}],"state":{}}</script></body>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text, Does.Contain("舊版 Nuxt 標題").And.Contain(LongBody));
    }

    [Test]
    public void ExtractFromHtml_GivenWindowNuxtFunctionForm_WhenExtracted_ThenStillHarvestsStringLiterals()
    {
        var html = $$$"""
            <body><div id="__nuxt"></div>
            <script>window.__NUXT__=(function(a,b){return {data:[{title:"函式形式標題內容",content:"{{{LongBody}}}"}],a:a}}(1,2));</script></body>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text, Does.Contain(LongBody));
    }

    [Test]
    public void ExtractFromHtml_GivenMalformedJsonEverywhere_WhenExtracted_ThenDoesNotThrow()
    {
        const string html = """
            <head><script type="application/ld+json">{"headline": "未結束</script>
            <script id="__NEXT_DATA__" type="application/json">{not json at all</script>
            <script id="__NUXT_DATA__">[1,2,</script>
            <script>window.__NUXT__={"a":</script></head><body><div id="root"></div></body>
            """;

        Assert.DoesNotThrow(() => WebPageHtmlTextExtractor.ExtractFromHtml(html));
    }

    [Test]
    public void ExtractFromHtml_GivenEnoughMainText_WhenExtracted_ThenIgnoresStructuredData()
    {
        var html = $$$"""
            <head><meta name="description" content="META-ONLY-DESCRIPTION"></head>
            <body><article><p>{{{LongParagraph}}}</p></article></body>
            """;

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text, Does.Not.Contain("META-ONLY-DESCRIPTION"));
    }

    [Test]
    public void ExtractFromHtml_GivenHugeStateStrings_WhenExtracted_ThenCapsTotalSize()
    {
        var strings = string.Join(",", Enumerable.Range(0, 5000).Select(i => $"\"這是第 {i} 段很長的可讀內容，用來測試總量上限的行為。\""));
        var html = $"<body><div id=\"__next\"></div><script id=\"__NEXT_DATA__\">[{strings}]</script></body>";

        var result = WebPageHtmlTextExtractor.ExtractFromHtml(html);

        Assert.That(result.Text.Length, Is.LessThanOrEqualTo(WebPageHtmlTextExtractor.MaxFallbackLength + 200));
    }

    [TestCase("""<body><div id="__next"></div></body>""")]
    [TestCase("""<body><div id="__nuxt"></div></body>""")]
    [TestCase("""<body><div id="app"></div></body>""")]
    [TestCase("""<body><div id="root"></div><script src="/bundle.js"></script></body>""")]
    [TestCase("""<body><noscript>You need to enable JavaScript to run this app.</noscript></body>""")]
    [TestCase("""<body><noscript>請啟用 JavaScript 以繼續使用本網站</noscript></body>""")]
    public void ExtractFromHtml_GivenClientRenderedShell_WhenExtracted_ThenFlagsAppShell(string html)
    {
        Assert.That(WebPageHtmlTextExtractor.ExtractFromHtml(html).LooksLikeAppShell, Is.True);
    }

    [Test]
    public void ExtractFromHtml_GivenOrdinaryShortPage_WhenExtracted_ThenNotFlagged()
    {
        Assert.That(WebPageHtmlTextExtractor.ExtractFromHtml("<body><p>short</p></body>").LooksLikeAppShell, Is.False);
    }
}
