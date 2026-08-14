using LabHub.Web.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabHub.Web.Pages;

public class NodeModel : PageModel
{
    private readonly ContentStore _store;

    public NodeModel(ContentStore store) => _store = store;

    public ContentNode? Node { get; private set; }

    public IReadOnlyList<(string Path, string Title)> Breadcrumbs { get; private set; } = [];

    public IReadOnlyList<ContentNode> PlainChildren { get; private set; } = [];

    public IReadOnlyList<IGrouping<string, ContentNode>> TrackGroups { get; private set; } = [];

    public IActionResult OnGet(string? path)
    {
        Node = _store.Find(path);
        if (Node is null) return NotFound();
        Breadcrumbs = BuildBreadcrumbs(Node.Path);
        PlainChildren = Node.Children.Where(c => c.Track is null).ToArray();
        TrackGroups = Node.Children
            .Where(c => c.Track is not null)
            .GroupBy(c => c.Track!)
            .ToArray();
        return Page();
    }

    private IReadOnlyList<(string, string)> BuildBreadcrumbs(string path)
    {
        var crumbs = new List<(string, string)>();
        var segments = path.Split('/');
        var prefix = "";
        foreach (var segment in segments[..^1])
        {
            prefix = prefix.Length == 0 ? segment : $"{prefix}/{segment}";
            if (_store.Find(prefix) is { } node) crumbs.Add((node.Path, node.Title));
        }
        return crumbs;
    }
}
