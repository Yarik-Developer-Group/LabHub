using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;

namespace LabHub.Web.Core;

/// <summary>Markdown → HTML. Безопасность: сырой HTML в markdown отключён
/// (экранируется как текст), опасные url-схемы в ссылках вычищаются,
/// относительные пути картинок переписываются на /assets/.</summary>
public static class MarkdownRenderer
{
    private static readonly MarkdownPipeline Pipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    public static string Render(string markdown, string assetBase)
    {
        var document = Markdown.Parse(markdown, Pipeline);
        RewriteInlines(document, assetBase);
        using var writer = new StringWriter();
        var renderer = new HtmlRenderer(writer);
        Pipeline.Setup(renderer);
        renderer.Render(document);
        writer.Flush();
        return writer.ToString();
    }

    private static void RewriteInlines(MarkdownDocument document, string assetBase)
    {
        foreach (var link in document.Descendants().OfType<LinkInline>())
        {
            if (IsDangerous(link.Url))
                link.Url = "#";
            else if (link.IsImage && IsRelative(link.Url))
                link.Url = $"{assetBase}/{NormalizeRelative(link.Url!)}";
        }
    }

    private static bool IsDangerous(string? url)
    {
        var trimmed = url?.TrimStart();
        return trimmed is not null
            && (trimmed.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("vbscript:", StringComparison.OrdinalIgnoreCase)
                || trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsRelative(string? url) =>
        !string.IsNullOrEmpty(url)
        && !url.StartsWith('#')
        && !url.StartsWith('/')
        && !url.Contains("://", StringComparison.Ordinal)
        && !url.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase);

    private static string NormalizeRelative(string url) =>
        url.StartsWith("./", StringComparison.Ordinal) ? url[2..] : url;
}
