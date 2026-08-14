using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabHub.Web.Pages;

public class DeniedModel : PageModel
{
    public void OnGet() => Response.StatusCode = StatusCodes.Status403Forbidden;
}
