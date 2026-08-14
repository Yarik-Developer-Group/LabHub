using LabHub.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LabHub.Tests;

/// <summary>Поднимает настоящий LabHub на случайном порту 127.0.0.1 —
/// интеграционные тесты ходят в него обычным HttpClient.</summary>
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
