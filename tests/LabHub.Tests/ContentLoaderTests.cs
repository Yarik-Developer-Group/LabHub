using LabHub.Web.Core;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LabHub.Tests;

public class ContentLoaderTests
{
    private static ContentLoader NewLoader() => new(NullLogger<ContentLoader>.Instance);

    private static string NewTempContent()
    {
        var dir = Path.Combine(Path.GetTempPath(), "labhub-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        return dir;
    }

    private static void WriteFile(string root, string relative, string text)
    {
        var path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, text);
    }

    [Fact]
    public void BuildsNestedTreeWithIndexAndLeafFiles()
    {
        var root = NewTempContent();
        WriteFile(root, "db/index.md", "---\ntitle: Базы данных\norder: 2\n---\nтело курса");
        WriteFile(root, "db/labs/index.md", "---\ntitle: Лабы\n---");
        WriteFile(root, "db/labs/lab-01.md", "---\ntitle: Лаба 1\norder: 1\n---\nтекст лабы");

        var snapshot = NewLoader().Load(root);

        Assert.Single(snapshot.Roots);
        Assert.Equal("Базы данных", snapshot.Roots[0].Title);
        Assert.Equal("Лаба 1", snapshot.Index["db/labs/lab-01"].Title);
        Assert.Contains("текст лабы", snapshot.Index["db/labs/lab-01"].HtmlBody);
    }

    [Fact]
    public void ChildrenAreSortedByOrderThenTitle()
    {
        var root = NewTempContent();
        WriteFile(root, "c/index.md", "---\ntitle: Курс\n---");
        WriteFile(root, "c/b-second.md", "---\ntitle: Б\norder: 2\n---");
        WriteFile(root, "c/a-first.md", "---\ntitle: А\norder: 1\n---");
        WriteFile(root, "c/z-also-first.md", "---\ntitle: В\norder: 1\n---");

        var course = NewLoader().Load(root).Roots[0];

        Assert.Equal("А", course.Children[0].Title);
        Assert.Equal("В", course.Children[1].Title);
        Assert.Equal("Б", course.Children[2].Title);
    }

    [Fact]
    public void TrackFromFrontMatterIsPreserved()
    {
        var root = NewTempContent();
        WriteFile(root, "c/extra/index.md", "---\ntitle: Экстра\n---");
        WriteFile(root, "c/extra/dev.md", "---\ntitle: Для разработчика\ntrack: Разработчик\n---");

        var snapshot = NewLoader().Load(root);

        Assert.Equal("Разработчик", snapshot.Index["c/extra/dev"].Track);
    }

    [Fact]
    public void FolderWithoutIndexGetsHumanizedTitle()
    {
        var root = NewTempContent();
        WriteFile(root, "computer-networks/labs/lab-01.md", "---\ntitle: Лаба\n---");

        var snapshot = NewLoader().Load(root);

        Assert.Equal("Computer networks", snapshot.Roots[0].Title);
        Assert.False(snapshot.Roots[0].HasBody);
    }

    [Fact]
    public void HiddenAndUnderscoreFoldersAreSkipped()
    {
        var root = NewTempContent();
        WriteFile(root, "visible/index.md", "---\ntitle: Видимый\n---");
        WriteFile(root, "_drafts/index.md", "---\ntitle: Черновики\n---");
        WriteFile(root, ".git/index.md", "мусор");

        var snapshot = NewLoader().Load(root);

        Assert.Single(snapshot.Roots);
        Assert.Equal("Видимый", snapshot.Roots[0].Title);
    }

    [Fact]
    public void AssetOnlyFoldersDoNotBecomePages()
    {
        var root = NewTempContent();
        WriteFile(root, "c/lab/index.md", "---\ntitle: Лаба\n---\n![схема](img/a.png)");
        WriteFile(root, "c/lab/img/a.png", "не-настоящий-png");

        var snapshot = NewLoader().Load(root);

        Assert.Empty(snapshot.Index["c/lab"].Children);
        Assert.False(snapshot.Index.ContainsKey("c/lab/img"));
    }

    [Fact]
    public void UnreadableEntryIsSkippedWithoutFailingWholeLoad()
    {
        if (OperatingSystem.IsWindows() || Environment.IsPrivilegedProcess) return;

        var root = NewTempContent();
        WriteFile(root, "ok/index.md", "---\ntitle: Хороший\n---");
        var locked = Path.Combine(root, "locked");
        Directory.CreateDirectory(locked);
        File.WriteAllText(Path.Combine(locked, "index.md"), "---\ntitle: Запертый\n---");
        var mode = File.GetUnixFileMode(locked);
        try
        {
            File.SetUnixFileMode(locked, UnixFileMode.None);

            var snapshot = NewLoader().Load(root);

            Assert.Single(snapshot.Roots);
            Assert.True(snapshot.WarningCount >= 1);
        }
        finally
        {
            File.SetUnixFileMode(locked, mode);
        }
    }

    [Fact]
    public void MissingRootDirectoryThrowsCleanly()
    {
        var missing = Path.Combine(Path.GetTempPath(), "labhub-tests", "нет-такой-папки");

        Assert.Throws<DirectoryNotFoundException>(() => NewLoader().Load(missing));
    }

    [Fact]
    public void ImagePathInLabIsRewrittenRelativeToItsFolder()
    {
        var root = NewTempContent();
        WriteFile(root, "db/labs/lab-01/index.md", "---\ntitle: Лаба\n---\n![схема](img/erd.png)");

        var snapshot = NewLoader().Load(root);

        Assert.Contains("src=\"/assets/db/labs/lab-01/img/erd.png\"", snapshot.Index["db/labs/lab-01"].HtmlBody);
    }
}
