namespace MarkdownEditor.Core.Workspace;

/// <summary>Scans a folder into a <see cref="WorkspaceNode"/> tree of Markdown files, for the workspace sidebar.</summary>
public static class WorkspaceScanner
{
    private static readonly string[] DefaultExcludedFolders = [".git", ".vs", ".vscode", "node_modules", "bin", "obj"];
    private static readonly string[] MarkdownExtensions = [".md", ".markdown"];

    /// <summary>Folders with no Markdown files anywhere beneath them are omitted, so the tree only shows what's relevant.</summary>
    public static WorkspaceNode Scan(string rootPath, IReadOnlyCollection<string>? excludedFolders = null)
    {
        ArgumentNullException.ThrowIfNull(rootPath);
        var excluded = excludedFolders ?? DefaultExcludedFolders;
        var dir = new DirectoryInfo(rootPath);
        return new WorkspaceNode(dir.Name, dir.FullName, IsFolder: true, Children: ScanChildren(dir, excluded));
    }

    private static List<WorkspaceNode> ScanChildren(DirectoryInfo dir, IReadOnlyCollection<string> excluded)
    {
        var result = new List<WorkspaceNode>();

        foreach (var subDir in dir.GetDirectories().OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (excluded.Contains(subDir.Name, StringComparer.OrdinalIgnoreCase))
                continue;
            var subChildren = ScanChildren(subDir, excluded);
            if (subChildren.Count > 0)
                result.Add(new WorkspaceNode(subDir.Name, subDir.FullName, IsFolder: true, Children: subChildren));
        }

        foreach (var file in dir.GetFiles().OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (MarkdownExtensions.Contains(file.Extension, StringComparer.OrdinalIgnoreCase))
                result.Add(new WorkspaceNode(file.Name, file.FullName, IsFolder: false, Children: []));
        }

        return result;
    }
}
