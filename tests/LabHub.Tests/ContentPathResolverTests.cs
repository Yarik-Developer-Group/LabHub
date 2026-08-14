using LabHub.Web.Core;

namespace LabHub.Tests;

public class ContentPathResolverTests
{
    [Test]
    public void AbsolutePathIsReturnedAsIs()
    {
        var абс = Path.Combine(Path.GetTempPath(), "labhub-abs");
        Check.Equal(Path.GetFullPath(абс), ContentPathResolver.Resolve("/anywhere", абс));
    }

    [Test]
    public void RelativePathIsFoundInParentDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "labhub-resolve", Guid.NewGuid().ToString("N"));
        var nested = Path.Combine(root, "src", "web");
        Directory.CreateDirectory(nested);
        Directory.CreateDirectory(Path.Combine(root, "content"));
        Check.Equal(Path.Combine(root, "content"), ContentPathResolver.Resolve(nested, "content"));
    }

    [Test]
    public void MissingRelativePathFallsBackToBase()
    {
        var basePath = Path.Combine(Path.GetTempPath(), "labhub-resolve-miss");
        Check.Equal(Path.GetFullPath(Path.Combine(basePath, "no-content")),
            ContentPathResolver.Resolve(basePath, "no-content"));
    }
}
