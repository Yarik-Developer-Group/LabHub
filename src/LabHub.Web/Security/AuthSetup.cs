using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;

namespace LabHub.Web.Security;

/// <summary>Google-вход через встроенный OAuth-хендлер ASP.NET Core (без внешних
/// пакетов). Если ClientId не задан — публичная часть работает, вход отключён.</summary>
public static class AuthSetup
{
    public const string GoogleScheme = "Google";

    public static bool IsConfigured(IConfiguration config) =>
        !string.IsNullOrWhiteSpace(config["Authentication:Google:ClientId"]);

    public static void Configure(WebApplicationBuilder builder)
    {
        var auth = builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme);
        auth.AddCookie(ConfigureCookie);
        if (IsConfigured(builder.Configuration))
            auth.AddOAuth(GoogleScheme, o => ConfigureGoogle(o, builder.Configuration));
        builder.Services.AddSingleton<IAuthorizationHandler, TeacherAuthorizationHandler>();
        builder.Services.AddAuthorization(o => o.AddPolicy(TeacherPolicy.Name,
            p => p.RequireAuthenticatedUser().AddRequirements(new TeacherRequirement())));
    }

    private static void ConfigureCookie(CookieAuthenticationOptions o)
    {
        o.LoginPath = "/login";
        o.AccessDeniedPath = "/denied";
        o.Cookie.Name = "labhub.auth";
        o.Cookie.HttpOnly = true;
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
    }

    private static void ConfigureGoogle(OAuthOptions o, IConfiguration config)
    {
        o.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        o.ClientId = config["Authentication:Google:ClientId"]!;
        o.ClientSecret = config["Authentication:Google:ClientSecret"] ?? "";
        o.CallbackPath = "/signin-google";
        o.AuthorizationEndpoint = "https://accounts.google.com/o/oauth2/v2/auth";
        o.TokenEndpoint = "https://oauth2.googleapis.com/token";
        o.UserInformationEndpoint = "https://openidconnect.googleapis.com/v1/userinfo";
        o.UsePkce = true;
        o.Scope.Add("openid");
        o.Scope.Add("email");
        o.Scope.Add("profile");
        o.ClaimActions.MapJsonKey(ClaimTypes.Email, "email");
        o.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
        o.Events = new OAuthEvents { OnCreatingTicket = FetchUserInfoAsync };
    }

    private static async Task FetchUserInfoAsync(OAuthCreatingTicketContext context)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", context.AccessToken);
        using var response = await context.Backchannel.SendAsync(request, context.HttpContext.RequestAborted);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(context.HttpContext.RequestAborted);
        using var user = JsonDocument.Parse(json);
        context.RunClaimActions(user.RootElement);
    }
}
