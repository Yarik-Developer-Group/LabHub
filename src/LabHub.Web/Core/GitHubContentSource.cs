using System.Formats.Tar;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace LabHub.Web.Core;

/// <summary>Скачивает контент из GitHub одним tarball-запросом и распаковывает
/// во временную папку. Используется для горячего обновления без рестарта.</summary>
public sealed partial class GitHubContentSource : IContentSource
{
    internal static readonly string TempPrefix = Path.Combine(Path.GetTempPath(), "labhub-content");

    private readonly HttpClient _http;
    private readonly IOptionsMonitor<LabHubOptions> _options;
    private readonly ILogger<GitHubContentSource> _logger;

    public GitHubContentSource(HttpClient http, IOptionsMonitor<LabHubOptions> options,
        ILogger<GitHubContentSource> logger)
    {
        _http = http;
        _options = options;
        _logger = logger;
    }

    public async Task<string> ResolveContentDirAsync(CancellationToken ct)
    {
        var gh = _options.CurrentValue.GitHub;
        ValidateRepo(gh.Repo);
        var url = $"https://api.github.com/repos/{gh.Repo}/tarball/{Uri.EscapeDataString(gh.Branch)}";
        using var request = BuildRequest(url, gh.Token);
        using var response = await _http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();
        var target = Directory.CreateDirectory(
            Path.Combine(TempPrefix, Guid.NewGuid().ToString("N"))).FullName;
        await ExtractAsync(response, target, ct);
        var dir = FindContentDir(target, gh.ContentDir);
        _logger.LogInformation("Fetched content from {Repo} into {Dir}", gh.Repo, dir);
        return dir;
    }

    private static void ValidateRepo(string? repo)
    {
        if (repo is null || !RepoPattern().IsMatch(repo))
            throw new InvalidOperationException("GitHub repo must look like 'owner/name'.");
    }

    private static HttpRequestMessage BuildRequest(string url, string? token)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd("LabHub/1.0");
        request.Headers.Accept.ParseAdd("application/vnd.github+json");
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return request;
    }

    private static async Task ExtractAsync(HttpResponseMessage response, string target, CancellationToken ct)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        await using var gzip = new GZipStream(stream, CompressionMode.Decompress);
        await TarFile.ExtractToDirectoryAsync(gzip, target, overwriteFiles: false, ct);
    }

    private static string FindContentDir(string target, string contentDir)
    {
        var top = Directory.EnumerateDirectories(target).SingleOrDefault()
            ?? throw new InvalidOperationException("Unexpected tarball layout.");
        var dir = Path.GetFullPath(Path.Combine(top, contentDir));
        if (!dir.StartsWith(top + Path.DirectorySeparatorChar, StringComparison.Ordinal) || !Directory.Exists(dir))
            throw new DirectoryNotFoundException($"Content dir '{contentDir}' not found in repository.");
        return dir;
    }

    [GeneratedRegex(@"^[\w.-]+/[\w.-]+$")]
    private static partial Regex RepoPattern();
}
