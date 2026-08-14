namespace LabHub.Web.Core;

/// <summary>Метаданные md-файла из front-matter блока между "---".</summary>
public sealed record FrontMatter
{
    public string? Title { get; init; }
    public int Order { get; init; }
    public string? Summary { get; init; }
    public string? Track { get; init; }
}

/// <summary>Разбор простого front-matter формата "key: value". Битый заголовок
/// не роняет разбор: файл считается обычным markdown без метаданных.</summary>
public static class FrontMatterParser
{
    public static (FrontMatter Meta, string Body) Parse(string text)
    {
        if (!TryExtractHeader(text, out var header, out var body))
            return (new FrontMatter(), text);
        return (FromPairs(ParsePairs(header)), body);
    }

    private static bool TryExtractHeader(string text, out string header, out string body)
    {
        header = "";
        body = text;
        if (!text.StartsWith("---", StringComparison.Ordinal)) return false;
        var end = text.IndexOf("\n---", 3, StringComparison.Ordinal);
        if (end < 0) return false;
        header = text[3..end];
        var afterLine = text.IndexOf('\n', end + 1);
        body = afterLine < 0 ? "" : text[(afterLine + 1)..];
        return true;
    }

    private static Dictionary<string, string> ParsePairs(string header)
    {
        var pairs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var raw in header.Split('\n'))
        {
            var line = raw.Trim();
            var colon = line.IndexOf(':');
            if (colon <= 0) continue;
            pairs[line[..colon].Trim()] = line[(colon + 1)..].Trim().Trim('"');
        }
        return pairs;
    }

    private static FrontMatter FromPairs(Dictionary<string, string> pairs) => new()
    {
        Title = pairs.GetValueOrDefault("title"),
        Summary = pairs.GetValueOrDefault("summary"),
        Track = pairs.GetValueOrDefault("track"),
        Order = int.TryParse(pairs.GetValueOrDefault("order"), out var order) ? order : 0
    };
}
