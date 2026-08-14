using LabHub.Web.Endpoints;

namespace LabHub.Tests;

public class AssetPathResolverTests
{
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "labhub-asset-tests");

    [Test]
    public void NormalPathResolvesInsideRoot()
    {
        Check.True(AssetPathResolver.TryResolve(Root, "db/labs/img/schema.png", out var full), "resolves");
        Check.True(full.StartsWith(Root, StringComparison.Ordinal), "inside root");
    }

    [Test]
    public void ParentTraversalIsBlocked()
    {
        Check.False(AssetPathResolver.TryResolve(Root, "../appsettings.json", out _), "..");
        Check.False(AssetPathResolver.TryResolve(Root, "db/../../etc/passwd", out _), "nested ..");
    }

    [Test]
    public void RootedAndNullBytePathsAreBlocked()
    {
        Check.False(AssetPathResolver.TryResolve(Root, "/etc/passwd", out _), "rooted");
        Check.False(AssetPathResolver.TryResolve(Root, "a\0b.png", out _), "null byte");
    }

    [Test]
    public void OnlyAllowlistedExtensionsGetContentType()
    {
        Check.Equal("image/png", AssetPathResolver.GetContentType("x/схема.png"));
        Check.Equal("image/svg+xml", AssetPathResolver.GetContentType("a.SVG"));
        Check.Equal(null, AssetPathResolver.GetContentType("evil.html"));
        Check.Equal(null, AssetPathResolver.GetContentType("lab.md"));
        Check.Equal(null, AssetPathResolver.GetContentType("noext"));
    }
}
