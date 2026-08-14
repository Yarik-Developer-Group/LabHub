namespace LabHub.Web.Core;

public sealed class LabHubOptions
{
    public const string SectionName = "LabHub";

    public string SiteName { get; set; } = "LabHub";

    public string ContentRoot { get; set; } = "content";

    public string[] Teachers { get; set; } = [];

    public string? WebhookSecret { get; set; }

    public int RefreshMinutes { get; set; }

    public GitHubOptions GitHub { get; set; } = new();
}

public sealed class GitHubOptions
{
    public string? Repo { get; set; }

    public string Branch { get; set; } = "main";

    public string? Token { get; set; }

    public string ContentDir { get; set; } = "content";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Repo);
}
