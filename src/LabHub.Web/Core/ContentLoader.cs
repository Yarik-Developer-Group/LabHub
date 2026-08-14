namespace LabHub.Web.Core;

/// <summary>Строит снимок контента из папки. Битый файл или папка не роняют
/// загрузку целиком: запись пропускается, предупреждение уходит в лог.</summary>
public sealed class ContentLoader
{
    private readonly ILogger<ContentLoader> _logger;

    public ContentLoader(ILogger<ContentLoader> logger) => _logger = logger;

    public ContentSnapshot Load(string rootPath)
    {
        var fullRoot = Path.GetFullPath(rootPath);
        if (!Directory.Exists(fullRoot))
            throw new DirectoryNotFoundException($"Content directory not found: {fullRoot}");
        var builder = new TreeBuilder(fullRoot, _logger);
        var roots = builder.LoadChildren(fullRoot, "");
        var index = new Dictionary<string, ContentNode>(StringComparer.OrdinalIgnoreCase);
        foreach (var root in roots) IndexTree(root, index);
        return new ContentSnapshot
        {
            Roots = roots,
            Index = index,
            AssetRoot = fullRoot,
            LoadedAtUtc = DateTimeOffset.UtcNow,
            WarningCount = builder.Warnings
        };
    }

    private static void IndexTree(ContentNode node, Dictionary<string, ContentNode> index)
    {
        index[node.Path] = node;
        foreach (var child in node.Children) IndexTree(child, index);
    }
}

internal sealed class TreeBuilder
{
    private readonly string _root;
    private readonly ILogger _logger;

    public TreeBuilder(string root, ILogger logger)
    {
        _root = root;
        _logger = logger;
    }

    public int Warnings { get; private set; }

    public IReadOnlyList<ContentNode> LoadChildren(string dir, string urlPath)
    {
        var nodes = new List<ContentNode>();
        foreach (var sub in Directory.EnumerateDirectories(dir).OrderBy(p => p, StringComparer.Ordinal))
            AddSafely(nodes, () => LoadDirectory(sub, urlPath));
        foreach (var file in Directory.EnumerateFiles(dir, "*.md").OrderBy(p => p, StringComparer.Ordinal))
            if (!IsIndex(file)) AddSafely(nodes, () => LoadFile(file, urlPath));
        return nodes
            .OrderBy(n => n.Order)
            .ThenBy(n => n.Title, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private void AddSafely(List<ContentNode> nodes, Func<ContentNode?> factory)
    {
        try
        {
            if (factory() is { } node) nodes.Add(node);
        }
        catch (Exception ex)
        {
            Warnings++;
            _logger.LogWarning(ex, "Skipping broken content entry");
        }
    }

    private ContentNode? LoadDirectory(string dir, string parentPath)
    {
        var slug = Slugify(Path.GetFileName(dir));
        if (IsHidden(slug)) return null;
        var path = CombinePath(parentPath, slug);
        var indexFile = Path.Combine(dir, "index.md");
        var (meta, body) = ReadDocument(indexFile);
        var children = LoadChildren(dir, path);
        // Папка без index.md и без md-потомков — хранилище картинок/файлов
        // лабы (img/ и т.п.), а не страница навигации.
        if (!File.Exists(indexFile) && children.Count == 0) return null;
        return new ContentNode
        {
            Slug = slug,
            Path = path,
            Title = meta.Title ?? Humanize(slug),
            Order = meta.Order,
            Summary = meta.Summary,
            Track = meta.Track,
            HtmlBody = RenderBody(body, dir),
            Children = children
        };
    }

    private ContentNode? LoadFile(string file, string parentPath)
    {
        var slug = Slugify(Path.GetFileNameWithoutExtension(file));
        if (IsHidden(slug)) return null;
        var (meta, body) = ReadDocument(file);
        return new ContentNode
        {
            Slug = slug,
            Path = CombinePath(parentPath, slug),
            Title = meta.Title ?? Humanize(slug),
            Order = meta.Order,
            Summary = meta.Summary,
            Track = meta.Track,
            HtmlBody = RenderBody(body, Path.GetDirectoryName(file)!)
        };
    }

    private static (FrontMatter Meta, string Body) ReadDocument(string file)
    {
        if (!File.Exists(file)) return (new FrontMatter(), "");
        return FrontMatterParser.Parse(File.ReadAllText(file));
    }

    private string RenderBody(string markdown, string dir)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return "";
        var rel = Path.GetRelativePath(_root, dir).Replace('\\', '/');
        var assetBase = rel == "." ? "/assets" : $"/assets/{rel}";
        return MarkdownRenderer.Render(markdown, assetBase);
    }

    private static string CombinePath(string parent, string slug) =>
        parent.Length == 0 ? slug : $"{parent}/{slug}";

    private static bool IsIndex(string file) =>
        Path.GetFileName(file).Equals("index.md", StringComparison.OrdinalIgnoreCase);

    private static bool IsHidden(string slug) =>
        slug.Length == 0 || slug.StartsWith('_') || slug.StartsWith('.');

    private static string Slugify(string name) => name.Trim().ToLowerInvariant();

    private static string Humanize(string slug) =>
        char.ToUpperInvariant(slug[0]) + slug[1..].Replace('-', ' ');
}
