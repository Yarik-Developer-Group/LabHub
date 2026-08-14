using LabHub.Web.Core;
using Xunit;

namespace LabHub.Tests;

public class MarkdownRendererTests
{
    [Fact]
    public void RendersBasicMarkdown()
    {
        var html = MarkdownRenderer.Render("## Заголовок\n\n**жирный** и `код`", "/assets");

        Assert.Contains("<h2", html);
        Assert.Contains("<strong>жирный</strong>", html);
        Assert.Contains("<code>код</code>", html);
    }

    [Fact]
    public void RawHtmlIsEscapedNotExecuted()
    {
        var html = MarkdownRenderer.Render("до <script>alert(1)</script> после", "/assets");

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Theory]
    [InlineData("[клик](javascript:alert(1))")]
    [InlineData("[клик](JavaScript:alert(1))")]
    [InlineData("[клик](vbscript:msgbox)")]
    [InlineData("![пиксель](data:image/png;base64,AAAA)")]
    public void DangerousUrlSchemesAreNeutralized(string markdown)
    {
        var html = MarkdownRenderer.Render(markdown, "/assets");

        Assert.DoesNotContain("javascript:", html);
        Assert.DoesNotContain("vbscript:", html);
        Assert.DoesNotContain("data:", html);
    }

    [Fact]
    public void RelativeImagePathIsRewrittenToAssets()
    {
        var html = MarkdownRenderer.Render("![схема](img/schema.png)", "/assets/db/labs/lab-1");

        Assert.Contains("src=\"/assets/db/labs/lab-1/img/schema.png\"", html);
    }

    [Fact]
    public void DotSlashImagePathIsNormalized()
    {
        var html = MarkdownRenderer.Render("![схема](./img/schema.png)", "/assets/x");

        Assert.Contains("src=\"/assets/x/img/schema.png\"", html);
    }

    [Fact]
    public void AbsoluteAndExternalUrlsAreLeftAlone()
    {
        var html = MarkdownRenderer.Render("![a](/assets/direct.png) [b](https://example.com)", "/assets/x");

        Assert.Contains("src=\"/assets/direct.png\"", html);
        Assert.Contains("href=\"https://example.com\"", html);
    }

    [Fact]
    public void PipeTablesAreRendered()
    {
        var html = MarkdownRenderer.Render("| a | b |\n|---|---|\n| 1 | 2 |", "/assets");

        Assert.Contains("<table>", html);
        Assert.Contains("<td>1</td>", html);
    }
}
