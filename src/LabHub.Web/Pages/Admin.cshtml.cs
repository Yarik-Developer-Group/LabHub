using System.Security.Claims;
using LabHub.Web.Core;
using LabHub.Web.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabHub.Web.Pages;

[Authorize(Policy = TeacherPolicy.Name)]
public class AdminModel : PageModel
{
    private readonly ContentStore _store;
    private readonly ContentRefresher _refresher;

    public AdminModel(ContentStore store, ContentRefresher refresher)
    {
        _store = store;
        _refresher = refresher;
    }

    public ContentSnapshot Snapshot => _store.Current;

    public string? LastError => _refresher.LastError;

    public string UserEmail => User.FindFirst(ClaimTypes.Email)?.Value ?? "?";

    [TempData]
    public string? Message { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostReloadAsync()
    {
        Message = await _refresher.RefreshAsync(HttpContext.RequestAborted)
            ? "Контент перезагружен."
            : "Не получилось обновить контент — подробности в логах.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostLogoutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/");
    }
}
