namespace MarkdownEditor.Core.Editing;

/// <summary>The outcome of a formatting operation: the new document text and the selection to restore.</summary>
public sealed record EditResult(string NewText, int NewSelectionStart, int NewSelectionLength);
