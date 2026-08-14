namespace LabHub.Web.Core;

/// <summary>Иммутабельный снимок всего контента. Заменяется целиком (atomic swap),
/// поэтому читатели никогда не видят полусобранное состояние.</summary>
public sealed record ContentSnapshot
{
    public required IReadOnlyList<ContentNode> Roots { get; init; }

    /// <summary>Индекс узлов по URL-пути (без регистра).</summary>
    public required IReadOnlyDictionary<string, ContentNode> Index { get; init; }

    /// <summary>Физическая папка, из которой раздаются картинки и файлы лаб.</summary>
    public string? AssetRoot { get; init; }

    public DateTimeOffset LoadedAtUtc { get; init; }

    /// <summary>Сколько битых файлов было пропущено при загрузке.</summary>
    public int WarningCount { get; init; }

    public static ContentSnapshot Empty { get; } = new()
    {
        Roots = [],
        Index = new Dictionary<string, ContentNode>()
    };
}
