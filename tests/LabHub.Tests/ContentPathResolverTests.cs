using LabHub.Web.Core;
using Xunit;

namespace LabHub.Tests;

public class ContentPathResolverTests
{
    [Fact]
    public void AbsolutePathIsReturnedAsIs()
    {
        var absolute = Path.Combine(Path.GetTempPath(), "labhub-abs");

        Assert.Equal(Path.GetFullPath(absolute), ContentPathResolver.Resolve("/anywhere", absolute));
    }

    [Fact]
    public void RelativePathIsFoundInParentDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "labhub-resolve", Guid.NewGuid().ToString("N"));
        var nested = Path.Combine(root, "src", "web");
        Directory.CreateDirectory(nested);
        Directory.CreateDirectory(Path.Combine(root, "content"));

        Assert.Equal(Path.Combine(root, "content"), ContentPathResolver.Resolve(nested, "content"));
    }

    [Fact]
    public void MissingRelativePathFallsBackToBase()
    {
        var basePath = Path.Combine(Path.GetTempPath(), "labhub-resolve-miss");

        Assert.Equal(
            Path.GetFullPath(Path.Combine(basePath, "no-content")),
            ContentPathResolver.Resolve(basePath, "no-content"));
    }
}
