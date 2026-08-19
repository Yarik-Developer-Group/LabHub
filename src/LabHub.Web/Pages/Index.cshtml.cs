using LabHub.Web.Core;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabHub.Web.Pages;

public class IndexModel : PageModel
{
    private readonly ContentStore _store;

    public IndexModel(ContentStore store) => _store = store;

    public bool HasContent { get; private set; }

    public void OnGet() => HasContent = _store.Current.Roots.Count > 0;
}
