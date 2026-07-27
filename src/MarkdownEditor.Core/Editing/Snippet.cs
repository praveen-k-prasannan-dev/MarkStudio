namespace MarkdownEditor.Core.Editing;

/// <summary>A text-expansion snippet: typing <see cref="Trigger"/> then pressing Tab expands it to <see cref="Expansion"/>. A <c>$0</c> token in the expansion marks where the caret lands afterward (defaults to the end).</summary>
public sealed record Snippet(string Trigger, string Expansion);
