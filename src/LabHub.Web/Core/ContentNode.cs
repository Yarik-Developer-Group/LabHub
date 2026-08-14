namespace LabHub.Web.Core;

public sealed record ContentNode
{
    public required string Slug { get; init; }

    public required string Path { get; init; }

    public required string Title { get; init; }

    public int Order { get; init; }

    public string? Summary { get; init; }

    public string? Track { get; init; }

    public string HtmlBody { get; init; } = "";

    public IReadOnlyList<ContentNode> Children { get; init; } = [];

    public bool HasBody => HtmlBody.Length > 0;
}
