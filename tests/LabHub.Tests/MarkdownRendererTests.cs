using LabHub.Web.Core;

namespace LabHub.Tests;

public class MarkdownRendererTests
{
    [Test]
    public void RendersBasicMarkdown()
    {
        var html = MarkdownRenderer.Render("## Заголовок\n\n**жирный** и `код`", "/assets");
        Check.Contains("<h2", html);
        Check.Contains("<strong>жирный</strong>", html);
        Check.Contains("<code>код</code>", html);
    }

    [Test]
    public void RawHtmlIsEscapedNotExecuted()
    {
        var html = MarkdownRenderer.Render("до <script>alert(1)</script> после", "/assets");
        Check.DoesNotContain("<script>", html);
        Check.Contains("&lt;script&gt;", html);
    }

    [Test]
    public void JavascriptUrlsAreNeutralized()
    {
        var html = MarkdownRenderer.Render("[клик](javascript:alert(1))", "/assets");
        Check.DoesNotContain("javascript:", html);
        Check.Contains("href=\"#\"", html);
    }

    [Test]
    public void RelativeImagePathIsRewrittenToAssets()
    {
        var html = MarkdownRenderer.Render("![схема](img/schema.png)", "/assets/db/labs/lab-1");
        Check.Contains("src=\"/assets/db/labs/lab-1/img/schema.png\"", html);
    }

    [Test]
    public void DotSlashImagePathIsNormalized()
    {
        var html = MarkdownRenderer.Render("![схема](./img/schema.png)", "/assets/x");
        Check.Contains("src=\"/assets/x/img/schema.png\"", html);
    }

    [Test]
    public void AbsoluteAndExternalUrlsAreLeftAlone()
    {
        var html = MarkdownRenderer.Render("![a](/assets/direct.png) [b](https://example.com)", "/assets/x");
        Check.Contains("src=\"/assets/direct.png\"", html);
        Check.Contains("href=\"https://example.com\"", html);
    }

    [Test]
    public void PipeTablesAreRendered()
    {
        var html = MarkdownRenderer.Render("| a | b |\n|---|---|\n| 1 | 2 |", "/assets");
        Check.Contains("<table>", html);
        Check.Contains("<td>1</td>", html);
    }
}
