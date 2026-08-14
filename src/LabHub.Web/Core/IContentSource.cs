namespace LabHub.Web.Core;

public interface IContentSource
{
    Task<string> ResolveContentDirAsync(CancellationToken ct);
}

public sealed class LocalContentSource : IContentSource
{
    private readonly string _path;

    public LocalContentSource(string path) => _path = path;

    public Task<string> ResolveContentDirAsync(CancellationToken ct) => Task.FromResult(_path);
}
