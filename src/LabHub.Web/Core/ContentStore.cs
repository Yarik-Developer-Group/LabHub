namespace LabHub.Web.Core;

/// <summary>Держатель текущего снимка контента. Swap атомарен: читатели видят
/// либо целиком старый, либо целиком новый контент, без блокировок.</summary>
public sealed class ContentStore
{
    private volatile ContentSnapshot _current = ContentSnapshot.Empty;

    public ContentSnapshot Current => _current;

    public void Swap(ContentSnapshot snapshot) =>
        _current = snapshot ?? throw new ArgumentNullException(nameof(snapshot));

    public ContentNode? Find(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        var key = path.Trim().Trim('/').ToLowerInvariant();
        return _current.Index.TryGetValue(key, out var node) ? node : null;
    }
}
