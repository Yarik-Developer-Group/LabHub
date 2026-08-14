namespace LabHub.Web.Endpoints;

public static class AssetPathResolver
{
    private static readonly Dictionary<string, string> ContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png",
        [".jpg"] = "image/jpeg",
        [".jpeg"] = "image/jpeg",
        [".gif"] = "image/gif",
        [".webp"] = "image/webp",
        [".svg"] = "image/svg+xml",
        [".pdf"] = "application/pdf",
        [".zip"] = "application/zip",
        [".txt"] = "text/plain; charset=utf-8"
    };

    public static bool TryResolve(string root, string relative, out string fullPath)
    {
        fullPath = "";
        if (relative.Contains("..", StringComparison.Ordinal)) return false;
        if (relative.Contains('\0') || Path.IsPathRooted(relative)) return false;
        var combined = Path.GetFullPath(Path.Combine(root, relative));
        if (!combined.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal)) return false;
        fullPath = combined;
        return true;
    }

    public static string? GetContentType(string path) =>
        ContentTypes.GetValueOrDefault(Path.GetExtension(path));
}
