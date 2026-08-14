using LabHub.Web.Core;
using Xunit;

namespace LabHub.Tests;

public class FrontMatterParserTests
{
    [Fact]
    public void ParsesTitleOrderTrackAndBody()
    {
        var text = "---\ntitle: Лаба 1\norder: 3\ntrack: Разработчик\nsummary: Кратко\n---\n# Тело\n";

        var (meta, body) = FrontMatterParser.Parse(text);

        Assert.Equal("Лаба 1", meta.Title);
        Assert.Equal(3, meta.Order);
        Assert.Equal("Разработчик", meta.Track);
        Assert.Equal("Кратко", meta.Summary);
        Assert.Contains("# Тело", body);
        Assert.DoesNotContain("---", body);
    }

    [Fact]
    public void TextWithoutFrontMatterIsAllBody()
    {
        var (meta, body) = FrontMatterParser.Parse("просто текст");

        Assert.Null(meta.Title);
        Assert.Equal("просто текст", body);
    }

    [Fact]
    public void UnclosedFrontMatterIsTreatedAsBody()
    {
        var text = "---\ntitle: без закрытия\nи дальше текст";

        var (meta, body) = FrontMatterParser.Parse(text);

        Assert.Null(meta.Title);
        Assert.Equal(text, body);
    }

    [Fact]
    public void BrokenOrderFallsBackToZero()
    {
        var (meta, _) = FrontMatterParser.Parse("---\norder: не число\n---\nтело");

        Assert.Equal(0, meta.Order);
    }

    [Fact]
    public void WindowsLineEndingsAreSupported()
    {
        var (meta, body) = FrontMatterParser.Parse("---\r\ntitle: CRLF\r\n---\r\nтело");

        Assert.Equal("CRLF", meta.Title);
        Assert.Contains("тело", body);
    }

    [Fact]
    public void LinesWithoutColonAreIgnored()
    {
        var (meta, _) = FrontMatterParser.Parse("---\nмусор без двоеточия\ntitle: ок\n---\nтело");

        Assert.Equal("ок", meta.Title);
    }
}
