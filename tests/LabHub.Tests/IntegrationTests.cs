using System.Net;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace LabHub.Tests;

public class IntegrationTests : IClassFixture<LabHubAppFixture>
{
    private readonly LabHubAppFixture _fixture;

    public IntegrationTests(LabHubAppFixture fixture) => _fixture = fixture;

    private HttpClient Client => _fixture.App.Client;

    private static string Sign(string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(LabHubAppFixture.WebhookSecret));
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

    [Fact]
    public async Task HomePageShowsWelcomeWithoutCourseCards()
    {
        var response = await Client.GetAsync("/");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("в меню зліва", html);
        Assert.DoesNotContain("Курс о сетях", html);
    }

    [Fact]
    public async Task LabPageRendersMarkdownWithBreadcrumbs()
    {
        var html = await Client.GetStringAsync("/c/nets/labs/lab-a");

        Assert.Contains("Лаба А", html);
        Assert.Contains("breadcrumbs", html);
        Assert.Contains(">Сети</a>", html);
        Assert.Contains("src=\"/assets/nets/labs/lab-a/img/pic.svg\"", html);
    }

    [Fact]
    public async Task RawHtmlInContentIsEscaped()
    {
        var html = await Client.GetStringAsync("/c/nets/labs/lab-a");

        Assert.DoesNotContain("<script>alert", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public async Task UnknownPageGivesFriendly404()
    {
        var response = await Client.GetAsync("/c/no/such/lab");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("не знайдено", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task HealthEndpointResponds()
    {
        Assert.Equal("ok", await Client.GetStringAsync("/healthz"));
    }

    [Fact]
    public async Task SecurityHeadersAreOnEveryResponse()
    {
        var response = await Client.GetAsync("/");

        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").First());
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").First());
        Assert.Contains("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").First());
    }

    [Fact]
    public async Task AssetIsServedWithSafeHeaders()
    {
        var response = await Client.GetAsync("/assets/nets/labs/lab-a/img/pic.svg");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("image/svg+xml", response.Content.Headers.ContentType!.MediaType);
        Assert.Contains("default-src 'none'", response.Headers.GetValues("Content-Security-Policy").First());
    }

    [Fact]
    public async Task AssetPathTraversalIsBlocked()
    {
        var response = await Client.GetAsync("/assets/%2e%2e/nets/index.md");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task MarkdownSourceIsNotDownloadable()
    {
        var response = await Client.GetAsync("/assets/nets/index.md");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task WebhookWithBadSignatureIsRejected()
    {
        var badSignature = await Client.SendAsync(WebhookRequest("{}", "sha256=" + new string('0', 64)));
        var noSignature = await Client.SendAsync(WebhookRequest("{}", null));

        Assert.Equal(HttpStatusCode.Forbidden, badSignature.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, noSignature.StatusCode);
    }

    [Fact]
    public async Task WebhookWithValidSignatureHotSwapsContent()
    {
        var before = await Client.GetAsync("/c/nets/labs/lab-b");
        Assert.Equal(HttpStatusCode.NotFound, before.StatusCode);

        LabHubAppFixture.WriteFile(_fixture.ContentDir, "nets/labs/lab-b.md",
            "---\ntitle: Лаба Б\n---\nНовая лаба без рестарта");
        var payload = """{"ref":"refs/heads/main"}""";
        var response = await Client.SendAsync(WebhookRequest(payload, Sign(payload)));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Новая лаба без рестарта", await Client.GetStringAsync("/c/nets/labs/lab-b"));
    }

    [Fact]
    public async Task WebhookIsHiddenWhenSecretNotConfigured()
    {
        var contentDir = LabHubAppFixture.CreateFixtureContent();
        await using var app = await TestApp.StartAsync(new Dictionary<string, string?>
        {
            ["LabHub:ContentRoot"] = contentDir,
            ["LabHub:RefreshMinutes"] = "0"
        });

        var response = await app.Client.SendAsync(WebhookRequest("{}", Sign("{}")));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AdminRedirectsAnonymousToLogin()
    {
        var response = await Client.GetAsync("/admin");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        Assert.Contains("/login", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task SidebarTreeShowsStructureWithActiveNode()
    {
        var html = await Client.GetStringAsync("/c/nets/labs/lab-a");

        Assert.Contains("class=\"tree\"", html);
        Assert.Contains("tree-link active", html);
        Assert.Contains("href=\"/c/nets/labs/lab-a\"", html);
        Assert.Contains("<details open", html);
    }

    [Fact]
    public async Task SidebarTreeIsPresentOnHomePage()
    {
        var html = await Client.GetStringAsync("/");

        Assert.Contains("class=\"tree\"", html);
        Assert.Contains(">Сети</a>", html);
    }

    [Fact]
    public async Task KatexAssetsAreServedAndPagesIncludeThem()
    {
        var js = await Client.GetAsync("/lib/katex/katex.min.js");
        var css = await Client.GetAsync("/lib/katex/katex.min.css");
        var html = await Client.GetStringAsync("/");

        Assert.Equal(HttpStatusCode.OK, js.StatusCode);
        Assert.Equal(HttpStatusCode.OK, css.StatusCode);
        Assert.Contains("js/math.js", html);
    }

    [Fact]
    public async Task LoginReportsAuthNotConfigured()
    {
        var response = await Client.GetAsync("/login");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
    }
}
