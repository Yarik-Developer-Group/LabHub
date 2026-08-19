using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabHub.Web.Pages;

[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public int Code { get; private set; }

    public string Heading { get; private set; } = "";

    public string Message { get; private set; } = "";

    public void OnGet(int? code) => Prepare(code);

    public void OnPost(int? code) => Prepare(code);

    private void Prepare(int? code)
    {
        Code = code is >= 400 and < 600 ? code.Value : 500;
        Response.StatusCode = Code;
        (Heading, Message) = Code switch
        {
            404 => ("Сторінку не знайдено", "Такої роботи чи розділу немає — можливо, посилання застаріло."),
            403 => ("Доступ заборонено", "Ця частина сайту доступна лише викладачам."),
            _ => ("Щось пішло не так", "Внутрішня помилка. Вона вже у логах, сайт продовжує працювати.")
        };
    }
}
