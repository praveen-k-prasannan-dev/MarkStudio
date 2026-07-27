using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using Markdig.Extensions.Mathematics;
using Markdig.Extensions.TaskLists;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using MTable = Markdig.Extensions.Tables.Table;
using MTableCell = Markdig.Extensions.Tables.TableCell;
using MTableRow = Markdig.Extensions.Tables.TableRow;
using WBreak = DocumentFormat.OpenXml.Wordprocessing.Break;
using WParagraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
using WRun = DocumentFormat.OpenXml.Wordprocessing.Run;
using WTable = DocumentFormat.OpenXml.Wordprocessing.Table;

namespace MarkdownEditor.Core.Export;

/// <summary>
/// Renders Markdown to a .docx file by walking Markdig's AST directly (not via the HTML output),
/// so formatting maps onto native Word runs/paragraphs instead of round-tripping through HTML.
/// Scope: headings, emphasis (bold/italic/strikethrough/highlight), inline code, links, bullet/
/// numbered/task lists (rendered as literal bullet/number/checkbox text, not native Word list
/// numbering - simpler and just as readable, at the cost of not being a "real" Word list you can
/// renumber), blockquotes (indentation), fenced/indented code blocks, horizontal rules, and tables.
/// Images become a "[Image: alt text]" placeholder rather than being embedded.
/// </summary>
public static class DocxExporter
{
    private readonly record struct RunStyle(
        bool Bold = false, bool Italic = false, bool Strike = false,
        bool Monospace = false, bool Highlight = false, int? FontSizeHalfPoints = null);

    public static void Export(string markdown, Stream outputStream)
    {
        ArgumentNullException.ThrowIfNull(markdown);
        ArgumentNullException.ThrowIfNull(outputStream);

        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().UseMathematics().Build();
        var document = Markdig.Markdown.Parse(markdown, pipeline);

        using var wordDocument = WordprocessingDocument.Create(outputStream, WordprocessingDocumentType.Document);
        var mainPart = wordDocument.AddMainDocumentPart();
        mainPart.Document = new Document();
        var body = mainPart.Document.AppendChild(new Body());

        foreach (var block in document)
            AppendBlock(body, mainPart, block);

        body.AppendChild(new SectionProperties());
        mainPart.Document.Save();
    }

    private static void AppendBlock(Body body, MainDocumentPart mainPart, Block block, int quoteDepth = 0)
    {
        switch (block)
        {
            case HeadingBlock heading:
                body.AppendChild(BuildParagraph(heading.Inline, mainPart, HeadingStyle(heading.Level), quoteDepth));
                break;

            case MTable table:
                body.AppendChild(BuildTable(table, mainPart));
                break;

            case ListBlock list:
                AppendList(body, mainPart, list, quoteDepth);
                break;

            case QuoteBlock quote:
                foreach (var child in quote)
                    AppendBlock(body, mainPart, child, quoteDepth + 1);
                break;

            case ThematicBreakBlock:
                body.AppendChild(BuildThematicBreak());
                break;

            case FencedCodeBlock or CodeBlock:
                body.AppendChild(BuildCodeParagraph((LeafBlock)block, quoteDepth));
                break;

            case ParagraphBlock paragraph:
                body.AppendChild(BuildParagraph(paragraph.Inline, mainPart, default, quoteDepth));
                break;

            case ContainerBlock container:
                foreach (var child in container)
                    AppendBlock(body, mainPart, child, quoteDepth);
                break;

            case LeafBlock leaf when leaf.Lines.Count > 0:
                body.AppendChild(new WParagraph(new WRun(new Text(leaf.Lines.ToString()))));
                break;
        }
    }

    private static RunStyle HeadingStyle(int level) => new(Bold: true, FontSizeHalfPoints: level switch
    {
        1 => 32,
        2 => 28,
        3 => 26,
        4 => 24,
        5 => 22,
        _ => 20,
    });

    private static void AppendList(Body body, MainDocumentPart mainPart, ListBlock list, int quoteDepth, int listDepth = 0)
    {
        int number = list.OrderedStart is not null && int.TryParse(list.OrderedStart, out int start) ? start : 1;

        foreach (var itemObj in list)
        {
            var item = (ListItemBlock)itemObj;
            ParagraphBlock? ownParagraph = null;
            ListBlock? nestedList = null;
            foreach (var child in item)
            {
                if (child is ParagraphBlock p && ownParagraph is null)
                    ownParagraph = p;
                else if (child is ListBlock nl)
                    nestedList = nl;
            }

            bool isTaskItem = ownParagraph?.Inline?.FirstChild is TaskList;
            string prefix = isTaskItem
                // No trailing space: Markdig's task-list inline leaves the required space between
                // "[ ]"/"[x]" and the item text as part of the following literal content.
                ? (((TaskList)ownParagraph!.Inline!.FirstChild!).Checked ? "☑" : "☐")
                : list.IsOrdered ? $"{number}. " : "• ";
            if (list.IsOrdered && !isTaskItem)
                number++;

            body.AppendChild(BuildParagraph(ownParagraph?.Inline, mainPart, default, quoteDepth, prefix, listDepth, skipFirstInline: isTaskItem));

            if (nestedList is not null)
                AppendList(body, mainPart, nestedList, quoteDepth, listDepth + 1);
        }
    }

    private static WParagraph BuildParagraph(
        ContainerInline? inline, MainDocumentPart mainPart, RunStyle baseStyle, int quoteDepth,
        string? listPrefix = null, int listDepth = 0, bool skipFirstInline = false)
    {
        var paragraph = new WParagraph();
        int indentLevel = quoteDepth + listDepth;

        if (indentLevel > 0 || listPrefix is not null)
        {
            var indentation = new Indentation { Left = ((indentLevel + (listPrefix is not null ? 1 : 0)) * 360).ToString() };
            if (listPrefix is not null)
                indentation.Hanging = "360";
            paragraph.AppendChild(new ParagraphProperties(indentation));
        }

        if (listPrefix is not null)
            paragraph.AppendChild(BuildRun(listPrefix, default));

        var start = skipFirstInline ? inline?.FirstChild?.NextSibling : inline;
        foreach (var run in BuildInlineRuns(start, mainPart, baseStyle))
            paragraph.AppendChild(run);

        return paragraph;
    }

    private static IEnumerable<OpenXmlElement> BuildInlineRuns(Inline? inline, MainDocumentPart mainPart, RunStyle style)
    {
        for (var current = inline; current is not null; current = current.NextSibling)
        {
            switch (current)
            {
                case TaskList:
                    continue;

                case LiteralInline literal:
                    yield return BuildRun(literal.Content.ToString(), style);
                    break;

                case CodeInline code:
                    yield return BuildRun(code.Content, style with { Monospace = true });
                    break;

                case MathInline math:
                    yield return BuildRun(math.Content.ToString(), style with { Monospace = true });
                    break;

                case LineBreakInline:
                    yield return new WRun(new WBreak());
                    break;

                case LinkInline { IsImage: true } image:
                    yield return BuildRun($"[Image: {ExtractPlainText(image)}]", style with { Italic = true });
                    break;

                case LinkInline link:
                    string relId = mainPart.AddHyperlinkRelationship(
                        new Uri(link.Url ?? "", UriKind.RelativeOrAbsolute), true).Id;
                    var linkRuns = BuildInlineRuns(link.FirstChild, mainPart, style).ToList();
                    if (linkRuns.Count == 0)
                        linkRuns.Add(BuildRun(link.Url ?? "", style));
                    yield return new Hyperlink(linkRuns) { History = true, Id = relId };
                    break;

                case EmphasisInline emphasis:
                    var childStyle = ApplyEmphasis(style, emphasis);
                    foreach (var r in BuildInlineRuns(emphasis.FirstChild, mainPart, childStyle))
                        yield return r;
                    break;

                case ContainerInline container:
                    foreach (var r in BuildInlineRuns(container.FirstChild, mainPart, style))
                        yield return r;
                    break;
            }
        }
    }

    private static RunStyle ApplyEmphasis(RunStyle style, EmphasisInline emphasis) => emphasis.DelimiterChar switch
    {
        '~' => style with { Strike = true },
        '=' => style with { Highlight = true },
        _ => emphasis.DelimiterCount >= 2 ? style with { Bold = true } : style with { Italic = true },
    };

    private static string ExtractPlainText(ContainerInline container)
    {
        var text = new System.Text.StringBuilder();
        for (var current = container.FirstChild; current is not null; current = current.NextSibling)
        {
            if (current is LiteralInline literal)
                text.Append(literal.Content.ToString());
            else if (current is ContainerInline nested)
                text.Append(ExtractPlainText(nested));
        }
        return text.ToString();
    }

    private static WRun BuildRun(string text, RunStyle style)
    {
        var props = new RunProperties();
        if (style.Bold) props.AppendChild(new Bold());
        if (style.Italic) props.AppendChild(new Italic());
        if (style.Strike) props.AppendChild(new Strike());
        if (style.Monospace) props.AppendChild(new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas" });
        if (style.Highlight) props.AppendChild(new Highlight { Val = HighlightColorValues.Yellow });
        if (style.FontSizeHalfPoints is { } size)
        {
            props.AppendChild(new FontSize { Val = size.ToString() });
            props.AppendChild(new FontSizeComplexScript { Val = size.ToString() });
        }

        var run = new WRun();
        if (props.HasChildren)
            run.AppendChild(props);
        run.AppendChild(new Text(text) { Space = SpaceProcessingModeValues.Preserve });
        return run;
    }

    private static WParagraph BuildCodeParagraph(LeafBlock block, int quoteDepth)
    {
        var pProps = new ParagraphProperties(
            new Shading { Val = ShadingPatternValues.Clear, Fill = "F0F0F0" });
        if (quoteDepth > 0)
            pProps.AppendChild(new Indentation { Left = (quoteDepth * 360).ToString() });

        var paragraph = new WParagraph(pProps);
        string[] lines = block.Lines.ToString().Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            paragraph.AppendChild(new WRun(
                new RunProperties(new RunFonts { Ascii = "Consolas", HighAnsi = "Consolas" }),
                new Text(lines[i].TrimEnd('\r')) { Space = SpaceProcessingModeValues.Preserve }));
            if (i < lines.Length - 1)
                paragraph.AppendChild(new WRun(new WBreak()));
        }
        return paragraph;
    }

    private static WParagraph BuildThematicBreak() => new(
        new ParagraphProperties(new ParagraphBorders(
            new BottomBorder { Val = BorderValues.Single, Size = 6, Color = "999999" })));

    private static WTable BuildTable(MTable mdTable, MainDocumentPart mainPart)
    {
        var table = new WTable(new TableProperties(new TableBorders(
            new TopBorder { Val = BorderValues.Single, Size = 4 },
            new BottomBorder { Val = BorderValues.Single, Size = 4 },
            new LeftBorder { Val = BorderValues.Single, Size = 4 },
            new RightBorder { Val = BorderValues.Single, Size = 4 },
            new InsideHorizontalBorder { Val = BorderValues.Single, Size = 4 },
            new InsideVerticalBorder { Val = BorderValues.Single, Size = 4 })));

        foreach (var rowObj in mdTable)
        {
            var mdRow = (MTableRow)rowObj;
            var row = new TableRow();
            foreach (var cellObj in mdRow)
            {
                var mdCell = (MTableCell)cellObj;
                var cellParagraphBlock = mdCell.OfType<ParagraphBlock>().FirstOrDefault();
                var style = mdRow.IsHeader ? new RunStyle(Bold: true) : default;
                row.AppendChild(new TableCell(BuildParagraph(cellParagraphBlock?.Inline, mainPart, style, 0)));
            }
            table.AppendChild(row);
        }
        return table;
    }
}
