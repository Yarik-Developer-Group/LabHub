using System.Text.Encodings.Web;
using System.Text.Unicode;
using LabHub.Web.Core;
using LabHub.Web.Endpoints;
using LabHub.Web.Security;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.WebEncoders;

namespace LabHub.Web;

public static class Startup
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        var services = builder.Services;
        services.Configure<LabHubOptions>(builder.Configuration.GetSection(LabHubOptions.SectionName));
        services.Configure<ForwardedHeadersOptions>(ConfigureForwardedHeaders);
        // Кириллица в HTML как текст, а не &#x...; — читабельно и компактнее.
        services.Configure<WebEncoderOptions>(o =>
            o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic));
        services.AddRazorPages();
        services.AddSingleton<ContentStore>();
        services.AddSingleton<ContentLoader>();
        services.AddHttpClient<GitHubContentSource>();
        services.AddSingleton<IContentSource>(ResolveContentSource);
        services.AddSingleton<ContentRefresher>();
        services.AddHostedService<ContentRefreshService>();
        AuthSetup.Configure(builder);
    }

    public static void ConfigurePipeline(WebApplication app)
    {
        app.UseForwardedHeaders();
        app.UseExceptionHandler("/error");
        app.UseStatusCodePagesWithReExecute("/error/{0}");
        if (!app.Environment.IsDevelopment()) app.UseHsts();
        app.UseHttpsRedirection();
        app.UseSecurityHeaders();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapRazorPages();
        ContentEndpoints.Map(app);
        AuthEndpoints.Map(app);
    }

    /// <summary>Первичная загрузка контента, приехавшего с деплоем. Ошибка не
    /// мешает старту: сайт поднимется пустым и добёрет контент из GitHub.</summary>
    public static void LoadInitialContent(WebApplication app)
    {
        try
        {
            var options = app.Services.GetRequiredService<IOptions<LabHubOptions>>().Value;
            var dir = ContentPathResolver.Resolve(app.Environment.ContentRootPath, options.ContentRoot);
            var snapshot = app.Services.GetRequiredService<ContentLoader>().Load(dir);
            app.Services.GetRequiredService<ContentStore>().Swap(snapshot);
            app.Logger.LogInformation("Content loaded from {Dir}: {Count} root sections, {Warnings} warnings",
                dir, snapshot.Roots.Count, snapshot.WarningCount);
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Initial content load failed; starting with empty content");
        }
    }

    private static IContentSource ResolveContentSource(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptionsMonitor<LabHubOptions>>().CurrentValue;
        if (options.GitHub.IsConfigured) return sp.GetRequiredService<GitHubContentSource>();
        var env = sp.GetRequiredService<IWebHostEnvironment>();
        return new LocalContentSource(ContentPathResolver.Resolve(env.ContentRootPath, options.ContentRoot));
    }

    private static void ConfigureForwardedHeaders(ForwardedHeadersOptions options)
    {
        // Azure App Service терминирует TLS на своём балансировщике.
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    }
}
