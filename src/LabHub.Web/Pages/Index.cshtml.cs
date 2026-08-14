using LabHub.Web.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabHub.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ContentStore _store;

    public IndexModel(ContentStore store) => _store = store;

    public IReadOnlyList<ContentNode> Roots { get; private set; } = [];

    public void OnGet() => Roots = _store.Current.Roots;
}
