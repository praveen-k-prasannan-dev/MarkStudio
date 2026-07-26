using System.Windows;
using System.Windows.Media;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using MarkdownEditor.Core.Spelling;

namespace MarkdownEditor.App.Services;

/// <summary>
/// Underlines misspelled words with a dotted red line - a simpler approximation of the usual
/// squiggly spell-check underline, which would otherwise need a custom wave-geometry brush.
/// </summary>
public sealed class SpellCheckColorizer : DocumentColorizingTransformer
{
    private static readonly TextDecorationCollection Decorations = CreateDecorations();

    public IReadOnlyList<MisspelledSpan> MisspelledSpans { get; set; } = [];

    protected override void ColorizeLine(DocumentLine line)
    {
        if (MisspelledSpans.Count == 0)
            return;

        int lineStart = line.Offset;
        int lineEnd = line.EndOffset;

        foreach (var span in MisspelledSpans)
        {
            int start = Math.Max(span.Start, lineStart);
            int end = Math.Min(span.Start + span.Length, lineEnd);
            if (start >= end)
                continue;

            ChangeLinePart(start, end, element => element.TextRunProperties.SetTextDecorations(Decorations));
        }
    }

    private static TextDecorationCollection CreateDecorations()
    {
        var pen = new Pen(Brushes.Red, 1.4) { DashStyle = DashStyles.Dot };
        pen.Freeze();

        var decoration = new TextDecoration
        {
            Location = TextDecorationLocation.Underline,
            Pen = pen,
            PenThicknessUnit = TextDecorationUnit.FontRecommended,
        };

        var collection = new TextDecorationCollection { decoration };
        collection.Freeze();
        return collection;
    }
}
