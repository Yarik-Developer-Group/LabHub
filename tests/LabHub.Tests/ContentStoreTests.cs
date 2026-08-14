using LabHub.Web.Core;
using Xunit;

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

    [Fact]
    public void StartsEmptyAndNeverNull()
    {
        var store = new ContentStore();

        Assert.NotNull(store.Current);
        Assert.Empty(store.Current.Roots);
    }

    [Fact]
    public void SwapReplacesSnapshotAtomically()
    {
        var store = new ContentStore();
        var first = SnapshotWith(Node("a"));
        var second = SnapshotWith(Node("b"));

        store.Swap(first);
        var seenBeforeSwap = store.Current;
        store.Swap(second);

        Assert.Equal("a", seenBeforeSwap.Roots[0].Path);
        Assert.Equal("b", store.Current.Roots[0].Path);
    }

    [Theory]
    [InlineData("/db/labs/lab-01/")]
    [InlineData("DB/Labs/LAB-01")]
    [InlineData("  db/labs/lab-01  ")]
    public void FindNormalizesSlashesCaseAndWhitespace(string query)
    {
        var store = new ContentStore();
        store.Swap(SnapshotWith(Node("db/labs/lab-01")));

        Assert.NotNull(store.Find(query));
    }

    [Theory]
    [InlineData("db/labs/nope")]
    [InlineData(null)]
    [InlineData("  ")]
    public void FindReturnsNullForUnknownOrEmptyPaths(string? query)
    {
        var store = new ContentStore();
        store.Swap(SnapshotWith(Node("db/labs/lab-01")));

        Assert.Null(store.Find(query));
    }
}
