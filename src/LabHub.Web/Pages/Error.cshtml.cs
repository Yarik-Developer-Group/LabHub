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
            404 => ("Страница не найдена", "Такой лабы или раздела нет — возможно, ссылка устарела."),
            403 => ("Доступ запрещён", "Эта часть сайта только для преподавателей."),
            _ => ("Что-то пошло не так", "Внутренняя ошибка. Она уже в логах, сайт продолжает работать.")
        };
    }
}
