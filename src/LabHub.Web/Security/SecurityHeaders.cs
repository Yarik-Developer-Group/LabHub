namespace LabHub.Web.Security;

/// <summary>Заголовки безопасности на каждый ответ. CSP строгий: только свои
/// ресурсы, без inline-скриптов; сайт нельзя встраивать во фреймы.</summary>
public static class SecurityHeaders
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app) =>
        app.Use(async (context, next) =>
        {
            Apply(context.Response.Headers);
            await next();
        });

    private static void Apply(IHeaderDictionary headers)
    {
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Content-Security-Policy"] =
            "default-src 'self'; img-src 'self' data:; style-src 'self'; script-src 'self'; " +
            "object-src 'none'; base-uri 'self'; frame-ancestors 'none'; form-action 'self'";
    }
}
