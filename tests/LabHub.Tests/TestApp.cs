using LabHub.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace LabHub.Tests;

public static class TestApp
{
    public static async Task<RunningApp> StartAsync(Dictionary<string, string?> settings)
    {
        var builder = WebApplication.CreateBuilder(BuildOptions());
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Logging.ClearProviders();
        builder.Configuration.AddInMemoryCollection(settings);
        Startup.ConfigureServices(builder);
        var app = builder.Build();
        Startup.ConfigurePipeline(app);
        Startup.LoadInitialContent(app);
        await app.StartAsync();
        return new RunningApp(app);
    }

    private static WebApplicationOptions BuildOptions() => new()
    {
        ApplicationName = typeof(Startup).Assembly.GetName().Name,
        ContentRootPath = LocateWebProjectDir(),
        EnvironmentName = "Production"
    };

    private static string LocateWebProjectDir()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "LabHub.sln")))
            dir = dir.Parent;
        if (dir is null) throw new InvalidOperationException("LabHub.sln not found above test binaries.");
        return Path.Combine(dir.FullName, "src", "LabHub.Web");
    }
}

public sealed class RunningApp : IAsyncDisposable
{
    public RunningApp(WebApplication app)
    {
        App = app;
        BaseUrl = app.Urls.First();
        Client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false })
        {
            BaseAddress = new Uri(BaseUrl)
        };
    }

    public WebApplication App { get; }

    public string BaseUrl { get; }

    public HttpClient Client { get; }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await App.StopAsync();
        await App.DisposeAsync();
    }
}

public sealed class LabHubAppFixture : IAsyncLifetime
{
    public const string WebhookSecret = "integration-secret";

    public RunningApp App { get; private set; } = null!;

    public string ContentDir { get; private set; } = "";

    public async Task InitializeAsync()
    {
        ContentDir = CreateFixtureContent();
        App = await TestApp.StartAsync(new Dictionary<string, string?>
        {
            ["LabHub:ContentRoot"] = ContentDir,
            ["LabHub:WebhookSecret"] = WebhookSecret,
            ["LabHub:RefreshMinutes"] = "0"
        });
    }

    public async Task DisposeAsync()
    {
        if (App is not null) await App.DisposeAsync();
    }

    public static string CreateFixtureContent()
    {
        var root = Path.Combine(Path.GetTempPath(), "labhub-tests", Guid.NewGuid().ToString("N"));
        WriteFile(root, "nets/index.md", "---\ntitle: Сети\norder: 1\nsummary: Курс о сетях\n---\nПро сети.");
        WriteFile(root, "nets/labs/index.md", "---\ntitle: Лабы\n---");
        WriteFile(root, "nets/labs/lab-a/index.md",
            "---\ntitle: Лаба А\n---\nТело лабы <script>alert(1)</script>\n\n![схема](img/pic.svg)");
        WriteFile(root, "nets/labs/lab-a/img/pic.svg",
            """<svg xmlns="http://www.w3.org/2000/svg" width="10" height="10"/>""");
        return root;
    }

    public static void WriteFile(string root, string relative, string text)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }
}
