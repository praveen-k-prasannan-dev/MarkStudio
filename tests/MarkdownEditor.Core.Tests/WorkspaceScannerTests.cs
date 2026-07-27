using MarkdownEditor.Core.Workspace;

namespace MarkdownEditor.Core.Tests;

public class WorkspaceScannerTests : IDisposable
{
    private readonly DirectoryInfo _root = Directory.CreateTempSubdirectory("mse-workspace-tests-");

    public void Dispose() => _root.Delete(recursive: true);

    private string Root => _root.FullName;

    private void WriteFile(string relativePath)
    {
        string full = Path.Combine(Root, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, "content");
    }

    [Fact]
    public void Finds_markdown_files_at_the_root()
    {
        WriteFile("notes.md");
        WriteFile("todo.markdown");
        WriteFile("ignored.txt");

        var tree = WorkspaceScanner.Scan(Root);

        Assert.Equal(2, tree.Children.Count);
        Assert.Contains(tree.Children, c => c.Name == "notes.md" && !c.IsFolder);
        Assert.Contains(tree.Children, c => c.Name == "todo.markdown" && !c.IsFolder);
    }

    [Fact]
    public void Recurses_into_subfolders()
    {
        WriteFile("projects/alpha.md");

        var tree = WorkspaceScanner.Scan(Root);

        var folder = Assert.Single(tree.Children);
        Assert.True(folder.IsFolder);
        Assert.Equal("projects", folder.Name);
        var file = Assert.Single(folder.Children);
        Assert.Equal("alpha.md", file.Name);
    }

    [Fact]
    public void Omits_folders_that_contain_no_markdown_files()
    {
        Directory.CreateDirectory(Path.Combine(Root, "empty-folder"));
        WriteFile("real.md");

        var tree = WorkspaceScanner.Scan(Root);

        Assert.DoesNotContain(tree.Children, c => c.Name == "empty-folder");
    }

    [Fact]
    public void Excludes_default_noise_folders_like_git_and_node_modules()
    {
        WriteFile(".git/HEAD.md"); // pathological but proves .git is skipped even if it somehow had .md files
        WriteFile("node_modules/pkg/readme.md");
        WriteFile("docs/real.md");

        var tree = WorkspaceScanner.Scan(Root);

        Assert.DoesNotContain(tree.Children, c => c.Name == ".git");
        Assert.DoesNotContain(tree.Children, c => c.Name == "node_modules");
        Assert.Contains(tree.Children, c => c.Name == "docs");
    }

    [Fact]
    public void Empty_workspace_returns_a_root_node_with_no_children()
    {
        var tree = WorkspaceScanner.Scan(Root);

        Assert.True(tree.IsFolder);
        Assert.Empty(tree.Children);
    }

    [Fact]
    public void Files_are_ordered_alphabetically()
    {
        WriteFile("zebra.md");
        WriteFile("apple.md");

        var tree = WorkspaceScanner.Scan(Root);

        Assert.Equal(["apple.md", "zebra.md"], tree.Children.Select(c => c.Name));
    }
}
