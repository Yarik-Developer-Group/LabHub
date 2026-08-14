namespace LabHub.Web.Core;

public static class ContentPathResolver
{
    public static string Resolve(string basePath, string configured)
    {
        if (Path.IsPathRooted(configured)) return Path.GetFullPath(configured);
        var probe = basePath;
        for (var i = 0; i < 4; i++)
        {
            var candidate = Path.Combine(probe, configured);
            if (Directory.Exists(candidate)) return Path.GetFullPath(candidate);
            probe = Path.GetDirectoryName(probe) ?? probe;
        }
        return Path.GetFullPath(Path.Combine(basePath, configured));
    }
}
