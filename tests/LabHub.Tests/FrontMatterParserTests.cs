using LabHub.Web.Core;

namespace LabHub.Tests;

public class FrontMatterParserTests
{
    [Test]
    public void ParsesTitleOrderTrackAndBody()
    {
        var text = "---\ntitle: Лаба 1\norder: 3\ntrack: Разработчик\nsummary: Кратко\n---\n# Тело\n";
        var (meta, body) = FrontMatterParser.Parse(text);
        Check.Equal("Лаба 1", meta.Title);
        Check.Equal(3, meta.Order);
        Check.Equal("Разработчик", meta.Track);
        Check.Equal("Кратко", meta.Summary);
        Check.Contains("# Тело", body);
        Check.DoesNotContain("---", body);
    }

    [Test]
    public void TextWithoutFrontMatterIsAllBody()
    {
        var (meta, body) = FrontMatterParser.Parse("просто текст");
        Check.Equal(null, meta.Title);
        Check.Equal("просто текст", body);
    }

    [Test]
    public void UnclosedFrontMatterIsTreatedAsBody()
    {
        var text = "---\ntitle: без закрытия\nи дальше текст";
        var (meta, body) = FrontMatterParser.Parse(text);
        Check.Equal(null, meta.Title);
        Check.Equal(text, body);
    }

    [Test]
    public void BrokenOrderFallsBackToZero()
    {
        var (meta, _) = FrontMatterParser.Parse("---\norder: не число\n---\nтело");
        Check.Equal(0, meta.Order);
    }

    [Test]
    public void WindowsLineEndingsAreSupported()
    {
        var (meta, body) = FrontMatterParser.Parse("---\r\ntitle: CRLF\r\n---\r\nтело");
        Check.Equal("CRLF", meta.Title);
        Check.Contains("тело", body);
    }

    [Test]
    public void LinesWithoutColonAreIgnored()
    {
        var (meta, _) = FrontMatterParser.Parse("---\nмусор без двоеточия\ntitle: ок\n---\nтело");
        Check.Equal("ок", meta.Title);
    }
}
