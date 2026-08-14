using LabHub.Web.Core;
using LabHub.Web.Security;
using Microsoft.Extensions.Options;

namespace LabHub.Web.Endpoints;

public static class ContentEndpoints
{
    private const long MaxWebhookPayloadBytes = 1_048_576;

    public static void Map(WebApplication app)
    {
        app.MapGet("/healthz", Health);
        app.MapPost("/api/content/refresh", RefreshAsync);
        app.MapGet("/assets/{**assetPath}", ServeAsset);
    }

    private static IResult Health() => Results.Text("ok");

    private static async Task<IResult> RefreshAsync(HttpContext http, ContentRefresher refresher)
    {
        var options = http.RequestServices.GetRequiredService<IOptionsMonitor<LabHubOptions>>();
        var secret = options.CurrentValue.WebhookSecret;
        if (string.IsNullOrWhiteSpace(secret)) return ApiError(404, "not found");
        if (http.Request.ContentLength is > MaxWebhookPayloadBytes)
            return ApiError(413, "payload too large");
        using var reader = new StreamReader(http.Request.Body);
        var payload = await reader.ReadToEndAsync(http.RequestAborted);
        var signature = http.Request.Headers["X-Hub-Signature-256"].ToString();
        if (!WebhookSignature.IsValid(payload, signature, secret))
            return ApiError(403, "invalid signature");
        if (await refresher.RefreshAsync(http.RequestAborted))
            return Results.Ok(new { status = "refreshed" });
        return ApiError(500, "content refresh failed");
    }

    private static IResult ApiError(int statusCode, string error) =>
    Results.Json(new { error }, statusCode: statusCode);

    private static IResult ServeAsset(string assetPath, ContentStore store, HttpContext http)
    {
        var root = store.Current.AssetRoot;
        if (root is null || string.IsNullOrWhiteSpace(assetPath)) return Results.NotFound();
        if (!AssetPathResolver.TryResolve(root, assetPath, out var fullPath)) return Results.NotFound();
        var contentType = AssetPathResolver.GetContentType(fullPath);
        if (contentType is null || !File.Exists(fullPath)) return Results.NotFound();
        http.Response.Headers.CacheControl = "public, max-age=300";
        http.Response.Headers.ContentSecurityPolicy = "default-src 'none'";
        return Results.File(fullPath, contentType);
    }
}
