using LabHub.Web.Core;

namespace LabHub.Tests;

public class ContentStoreTests
{
    private static ContentSnapshot SnapshotWith(params ContentNode[] nodes) => new()
    {
        Roots = nodes,
        Index = nodes.ToDictionary(n => n.Path, n => n, StringComparer.OrdinalIgnoreCase),
        AssetRoot = "/tmp/does-not-matter"
    };

    private static ContentNode Node(string path) => new()
    {
        Slug = path.Split('/')[^1],
        Path = path,
        Title = path
    };

    [Test]
    public void StartsEmptyAndNeverNull()
    {
        var store = new ContentStore();
        Check.NotNull(store.Current, "initial snapshot");
        Check.Equal(0, store.Current.Roots.Count);
    }

    [Test]
    public void SwapReplacesSnapshotAtomically()
    {
        var store = new ContentStore();
        var first = SnapshotWith(Node("a"));
        var second = SnapshotWith(Node("b"));
        store.Swap(first);
        var seenBeforeSwap = store.Current;
        store.Swap(second);
        Check.Equal("a", seenBeforeSwap.Roots[0].Path);
        Check.Equal("b", store.Current.Roots[0].Path);
    }

    [Test]
    public void FindNormalizesSlashesAndCase()
    {
        var store = new ContentStore();
        store.Swap(SnapshotWith(Node("db/labs/lab-01")));
        Check.NotNull(store.Find("/db/labs/lab-01/"), "с обрамляющими слэшами");
        Check.NotNull(store.Find("DB/Labs/LAB-01"), "без учёта регистра");
        Check.Equal(null, store.Find("db/labs/nope"));
        Check.Equal(null, store.Find(null));
        Check.Equal(null, store.Find("  "));
    }
}
