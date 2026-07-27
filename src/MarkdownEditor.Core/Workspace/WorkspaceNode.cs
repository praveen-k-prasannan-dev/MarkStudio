namespace MarkdownEditor.Core.Workspace;

/// <summary>A folder or Markdown file in a workspace tree. <see cref="IsFolder"/> distinguishes the two; files have no children.</summary>
public sealed record WorkspaceNode(string Name, string FullPath, bool IsFolder, IReadOnlyList<WorkspaceNode> Children);
