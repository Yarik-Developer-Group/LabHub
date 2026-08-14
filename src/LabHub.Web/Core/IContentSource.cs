namespace LabHub.Web.Core;

/// <summary>Источник контента: отвечает на вопрос «из какой папки грузить».</summary>
public interface IContentSource
{
    Task<string> ResolveContentDirAsync(CancellationToken ct);
}

/// <summary>Локальная папка (контент, приехавший вместе с деплоем).</summary>
public sealed class LocalContentSource : IContentSource
{
    private readonly string _path;

    public LocalContentSource(string path) => _path = path;

    public Task<string> ResolveContentDirAsync(CancellationToken ct) => Task.FromResult(_path);
}
