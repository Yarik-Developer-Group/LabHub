using LabHub.Web.Core;

namespace LabHub.Web.Pages;

public sealed record TreeNodeView(ContentNode Node, string CurrentPath)
{
    public bool IsActive =>
        Node.Path.Equals(CurrentPath, StringComparison.OrdinalIgnoreCase);

    public bool IsOnCurrentPath =>
        IsActive || CurrentPath.StartsWith(Node.Path + "/", StringComparison.OrdinalIgnoreCase);
}
