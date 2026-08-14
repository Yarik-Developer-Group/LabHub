namespace LabHub.Web.Core;

/// <summary>Настройки LabHub. Секреты сюда не попадают из appsettings.json —
/// они приходят из user-secrets (локально) или App Settings (Azure).</summary>
public sealed class LabHubOptions
{
    public const string SectionName = "LabHub";

    public string SiteName { get; set; } = "LabHub";

    /// <summary>Папка с контентом. Относительный путь ищется от корня приложения вверх.</summary>
    public string ContentRoot { get; set; } = "content";

    /// <summary>Email-адреса преподавателей (allowlist для админки).</summary>
    public string[] Teachers { get; set; } = [];

    /// <summary>Секрет GitHub-webhook. Пусто — endpoint обновления отключён.</summary>
    public string? WebhookSecret { get; set; }

    /// <summary>Период фоновой сверки контента с GitHub, минут. 0 — выключено.</summary>
    public int RefreshMinutes { get; set; }

    public GitHubOptions GitHub { get; set; } = new();
}

public sealed class GitHubOptions
{
    /// <summary>Репозиторий вида "owner/name". Пусто — контент только из локальной папки.</summary>
    public string? Repo { get; set; }

    public string Branch { get; set; } = "main";

    /// <summary>Токен для GitHub API (опционально, снимает жёсткий rate-limit).</summary>
    public string? Token { get; set; }

    /// <summary>Подпапка с контентом внутри репозитория.</summary>
    public string ContentDir { get; set; } = "content";

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Repo);
}
