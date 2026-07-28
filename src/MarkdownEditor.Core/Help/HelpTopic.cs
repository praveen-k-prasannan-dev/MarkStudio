namespace MarkdownEditor.Core.Help;

/// <summary>A node in the Help window's topic tree - either a category (no <see cref="File"/>, has children) or a leaf topic (has a <see cref="File"/>).</summary>
public sealed class HelpTopic
{
    public required string Title { get; init; }
    public string? File { get; init; }
    public IReadOnlyList<HelpTopic> Children { get; init; } = [];
}
