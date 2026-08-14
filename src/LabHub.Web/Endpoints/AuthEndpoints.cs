using LabHub.Web.Security;
using Microsoft.AspNetCore.Authentication;

namespace LabHub.Web.Endpoints;

public static class AuthEndpoints
{
    public static void Map(WebApplication app)
    {
        if (AuthSetup.IsConfigured(app.Configuration))
            app.MapGet("/login", ChallengeGoogle);
        else
            app.MapGet("/login", NotConfigured);
    }

    private static IResult ChallengeGoogle() =>
        Results.Challenge(new AuthenticationProperties { RedirectUri = "/admin" },
            [AuthSetup.GoogleScheme]);

    private static IResult NotConfigured() =>
        Results.Text("Вход не настроен: задайте Authentication:Google:ClientId и ClientSecret.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
}
