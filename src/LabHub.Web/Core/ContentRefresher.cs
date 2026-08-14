namespace LabHub.Web.Core;

/// <summary>Оркестратор обновления: источник → загрузка → атомарная подмена.
/// Любая ошибка не роняет приложение — старый снимок продолжает работать.</summary>
public sealed class ContentRefresher
{
    private readonly IContentSource _source;
    private readonly ContentLoader _loader;
    private readonly ContentStore _store;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public ContentRefresher(IContentSource source, ContentLoader loader, ContentStore store)
    {
        _source = source;
        _loader = loader;
        _store = store;
    }

    public string? LastError { get; private set; }

    public async Task<bool> RefreshAsync(CancellationToken ct)
    {
        await _gate.WaitAsync(ct);
        try
        {
            var dir = await _source.ResolveContentDirAsync(ct);
            var previousRoot = _store.Current.AssetRoot;
            _store.Swap(_loader.Load(dir));
            CleanupPrevious(previousRoot, _store.Current.AssetRoot);
            LastError = null;
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            return false;
        }
        finally
        {
            _gate.Release();
        }
    }

    private static void CleanupPrevious(string? previous, string? current)
    {
        if (previous is null || previous == current) return;
        if (!previous.StartsWith(GitHubContentSource.TempPrefix, StringComparison.Ordinal)) return;
        try
        {
            Directory.Delete(GetDownloadRoot(previous), recursive: true);
        }
        catch (Exception)
        {
            // Мусор во временной папке не стоит ошибки: приберём в следующий раз.
        }
    }

    private static string GetDownloadRoot(string contentDir)
    {
        var dir = new DirectoryInfo(contentDir);
        while (dir.Parent is not null && dir.Parent.FullName != GitHubContentSource.TempPrefix)
            dir = dir.Parent;
        return dir.FullName;
    }
}
