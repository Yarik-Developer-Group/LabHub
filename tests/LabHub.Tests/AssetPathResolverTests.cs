using LabHub.Web.Endpoints;
using Xunit;

namespace LabHub.Tests;

public class AssetPathResolverTests
{
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "labhub-asset-tests");

    [Fact]
    public void NormalPathResolvesInsideRoot()
    {
        Assert.True(AssetPathResolver.TryResolve(Root, "db/labs/img/schema.png", out var full));
        Assert.StartsWith(Root, full, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("../appsettings.json")]
    [InlineData("db/../../etc/passwd")]
    [InlineData("/etc/passwd")]
    [InlineData("a\0b.png")]
    public void TraversalRootedAndNullBytePathsAreBlocked(string path)
    {
        Assert.False(AssetPathResolver.TryResolve(Root, path, out _));
    }

    [Theory]
    [InlineData("x/схема.png", "image/png")]
    [InlineData("a.SVG", "image/svg+xml")]
    [InlineData("doc.pdf", "application/pdf")]
    public void AllowlistedExtensionsGetContentType(string path, string expected)
    {
        Assert.Equal(expected, AssetPathResolver.GetContentType(path));
    }

    [Theory]
    [InlineData("evil.html")]
    [InlineData("lab.md")]
    [InlineData("noext")]
    [InlineData("script.cs")]
    public void OtherExtensionsAreRejected(string path)
    {
        Assert.Null(AssetPathResolver.GetContentType(path));
    }
}
