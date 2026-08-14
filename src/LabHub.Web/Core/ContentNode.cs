namespace LabHub.Web.Core;

/// <summary>Узел дерева контента: курс, раздел, статья или лаба. Иммутабелен.</summary>
public sealed record ContentNode
{
    public required string Slug { get; init; }

    /// <summary>URL-путь узла относительно /c/, например "databases/labs/lab-02-sql".</summary>
    public required string Path { get; init; }

    public required string Title { get; init; }

    public int Order { get; init; }

    public string? Summary { get; init; }

    /// <summary>Трек («Разработчик», «Администратор БД»...) для экстра-материалов.</summary>
    public string? Track { get; init; }

    /// <summary>Отрендеренный HTML тела. Пустая строка — узел без собственного текста.</summary>
    public string HtmlBody { get; init; } = "";

    public IReadOnlyList<ContentNode> Children { get; init; } = [];

    public bool HasBody => HtmlBody.Length > 0;
}
