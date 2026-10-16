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
}
