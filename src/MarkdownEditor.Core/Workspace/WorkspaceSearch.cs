namespace MarkdownEditor.Core.Workspace;

public readonly record struct WorkspaceSearchResult(string FilePath, int LineNumber, string LineText);

/// <summary>Plain-text search across every file in a workspace, line by line. The caller supplies file contents so this stays disk-free and testable.</summary>
public static class WorkspaceSearch
{
    public static IReadOnlyList<WorkspaceSearchResult> Search(IEnumerable<(string Path, string Content)> files, string query)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(query);
        if (query.Length == 0)
            return [];

        var results = new List<WorkspaceSearchResult>();
        foreach (var (path, content) in files)
        {
            string[] lines = content.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains(query, StringComparison.OrdinalIgnoreCase))
                    results.Add(new WorkspaceSearchResult(path, i + 1, lines[i].Trim('\r', ' ', '\t')));
            }
        }
        return results;
    }
}
