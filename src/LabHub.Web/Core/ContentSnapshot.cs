namespace LabHub.Web.Core;

public sealed record ContentSnapshot
{
    public required IReadOnlyList<ContentNode> Roots { get; init; }

    public required IReadOnlyDictionary<string, ContentNode> Index { get; init; }

    public string? AssetRoot { get; init; }

    public DateTimeOffset LoadedAtUtc { get; init; }

    public int WarningCount { get; init; }

    public static ContentSnapshot Empty { get; } = new()
    {
        Roots = [],
        Index = new Dictionary<string, ContentNode>()
    };
}
