using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace LabHub.Tests;

/// <summary>Интеграционные тесты против живого приложения на локальном порту.
/// Одно приложение на весь класс: MicroTest выполняет тесты последовательно.</summary>
public class IntegrationTests
{
    private const string Secret = "integration-secret";

    private static RunningApp? _shared;
    private static string _contentDir = "";

    private static async Task<RunningApp> AppAsync()
    {
        if (_shared is not null) return _shared;
        _contentDir = CreateFixtureContent();
        _shared = await TestApp.StartAsync(new Dictionary<string, string?>
        {
            ["LabHub:ContentRoot"] = _contentDir,
            ["LabHub:WebhookSecret"] = Secret,
            ["LabHub:RefreshMinutes"] = "0"
        });
        return _shared;
    }

    private static string CreateFixtureContent()
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

    private static void WriteFile(string root, string relative, string text)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    private static string Sign(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(Secret));
        return "sha256=" + Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLowerInvariant();
    }

    private static HttpRequestMessage WebhookRequest(string payload, string? signature)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/content/refresh")
        {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };
        if (signature is not null) request.Headers.Add("X-Hub-Signature-256", signature);
        return request;
    }

    [Test]
    public async Task HomePageListsCourses()
    {
        var app = await AppAsync();
        var response = await app.Client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();
        Check.Equal(HttpStatusCode.OK, response.StatusCode);
        Check.Contains("Сети", html);
        Check.Contains("Курс о сетях", html);
    }

    [Test]
    public async Task LabPageRendersMarkdownWithBreadcrumbs()
    {
        var app = await AppAsync();
        var html = await app.Client.GetStringAsync("/c/nets/labs/lab-a");
        Check.Contains("Лаба А", html);
        Check.Contains("breadcrumbs", html);
        Check.Contains(">Сети</a>", html);
        Check.Contains("src=\"/assets/nets/labs/lab-a/img/pic.svg\"", html);
    }

    [Test]
    public async Task RawHtmlInContentIsEscaped()
    {
        var app = await AppAsync();
        var html = await app.Client.GetStringAsync("/c/nets/labs/lab-a");
        Check.DoesNotContain("<script>alert", html);
        Check.Contains("&lt;script&gt;", html);
    }

    [Test]
    public async Task UnknownPageGivesFriendly404()
    {
        var app = await AppAsync();
        var response = await app.Client.GetAsync("/c/no/such/lab");
        Check.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Check.Contains("не найдена", await response.Content.ReadAsStringAsync());
    }

    [Test]
    public async Task HealthEndpointResponds()
    {
        var app = await AppAsync();
        Check.Equal("ok", await app.Client.GetStringAsync("/healthz"));
    }

    [Test]
    public async Task SecurityHeadersAreOnEveryResponse()
    {
        var app = await AppAsync();
        var response = await app.Client.GetAsync("/");
        Check.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
        Check.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
        Check.Contains("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").First());
    }

    [Test]
    public async Task AssetIsServedWithSafeHeaders()
    {
        var app = await AppAsync();
        var response = await app.Client.GetAsync("/assets/nets/labs/lab-a/img/pic.svg");
        Check.Equal(HttpStatusCode.OK, response.StatusCode);
        Check.Equal("image/svg+xml", response.Content.Headers.ContentType!.MediaType);
        Check.Contains("default-src 'none'", response.Headers.GetValues("Content-Security-Policy").First());
    }

    [Test]
    public async Task AssetPathTraversalIsBlocked()
    {
        var app = await AppAsync();
        var response = await app.Client.GetAsync("/assets/%2e%2e/nets/index.md");
        Check.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task MarkdownSourceIsNotDownloadable()
    {
        var app = await AppAsync();
        var response = await app.Client.GetAsync("/assets/nets/index.md");
        Check.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task WebhookWithBadSignatureIsRejected()
    {
        var app = await AppAsync();
        var response = await app.Client.SendAsync(WebhookRequest("{}", "sha256=" + new string('0', 64)));
        Check.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        var missing = await app.Client.SendAsync(WebhookRequest("{}", null));
        Check.Equal(HttpStatusCode.Forbidden, missing.StatusCode);
    }

    [Test]
    public async Task WebhookWithValidSignatureHotSwapsContent()
    {
        var app = await AppAsync();
        var before = await app.Client.GetAsync("/c/nets/labs/lab-b");
        Check.Equal(HttpStatusCode.NotFound, before.StatusCode);
        WriteFile(_contentDir, "nets/labs/lab-b.md", "---\ntitle: Лаба Б\n---\nНовая лаба без рестарта");
        var payload = """{"ref":"refs/heads/main"}""";
        var response = await app.Client.SendAsync(WebhookRequest(payload, Sign(payload)));
        Check.Equal(HttpStatusCode.OK, response.StatusCode);
        var after = await app.Client.GetStringAsync("/c/nets/labs/lab-b");
        Check.Contains("Новая лаба без рестарта", after);
    }

    [Test]
    public async Task WebhookIsHiddenWhenSecretNotConfigured()
    {
        var contentDir = CreateFixtureContent();
        await using var app = await TestApp.StartAsync(new Dictionary<string, string?>
        {
            ["LabHub:ContentRoot"] = contentDir,
            ["LabHub:RefreshMinutes"] = "0"
        });
        var response = await app.Client.SendAsync(WebhookRequest("{}", Sign("{}")));
        Check.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Test]
    public async Task AdminRedirectsAnonymousToLogin()
    {
        var app = await AppAsync();
        var response = await app.Client.GetAsync("/admin");
        Check.Equal(HttpStatusCode.Found, response.StatusCode);
        Check.Contains("/login", response.Headers.Location!.ToString());
    }

    [Test]
    public async Task LoginReportsAuthNotConfigured()
    {
        var app = await AppAsync();
        var response = await app.Client.GetAsync("/login");
        Check.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
