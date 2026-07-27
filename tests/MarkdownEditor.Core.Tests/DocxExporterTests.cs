using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using MarkdownEditor.Core.Export;

namespace MarkdownEditor.Core.Tests;

public class DocxExporterTests
{
    private static Body ExportAndReopen(string markdown)
    {
        using var stream = new MemoryStream();
        DocxExporter.Export(markdown, stream);

        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        // Clone the body so it survives the WordprocessingDocument (and its stream) being disposed.
        return (Body)doc.MainDocumentPart!.Document!.Body!.CloneNode(true);
    }

    [Fact]
    public void Plain_paragraph_text_is_preserved()
    {
        var body = ExportAndReopen("Hello world.");

        Assert.Contains("Hello world.", body.InnerText);
    }

    [Fact]
    public void Heading_text_is_bold_and_sized_by_level()
    {
        var body = ExportAndReopen("# Title");

        var run = body.Descendants<Run>().First(r => r.InnerText == "Title");
        Assert.NotNull(run.RunProperties?.Bold);
        Assert.Equal("32", run.RunProperties!.FontSize!.Val);
    }

    [Fact]
    public void Bold_text_gets_a_bold_run()
    {
        var body = ExportAndReopen("Some **bold** text.");

        var run = body.Descendants<Run>().First(r => r.InnerText == "bold");
        Assert.NotNull(run.RunProperties?.Bold);
    }

    [Fact]
    public void Italic_text_gets_an_italic_run()
    {
        var body = ExportAndReopen("Some *italic* text.");

        var run = body.Descendants<Run>().First(r => r.InnerText == "italic");
        Assert.NotNull(run.RunProperties?.Italic);
    }

    [Fact]
    public void Strikethrough_text_gets_a_strike_run()
    {
        var body = ExportAndReopen("Some ~~struck~~ text.");

        var run = body.Descendants<Run>().First(r => r.InnerText == "struck");
        Assert.NotNull(run.RunProperties?.Strike);
    }

    [Fact]
    public void Highlighted_text_gets_a_highlight_run()
    {
        var body = ExportAndReopen("Some ==marked== text.");

        var run = body.Descendants<Run>().First(r => r.InnerText == "marked");
        Assert.NotNull(run.RunProperties?.Highlight);
    }

    [Fact]
    public void Inline_code_uses_a_monospace_font()
    {
        var body = ExportAndReopen("Run `code` here.");

        var run = body.Descendants<Run>().First(r => r.InnerText == "code");
        Assert.Equal("Consolas", run.RunProperties!.RunFonts!.Ascii);
    }

    [Fact]
    public void Bullet_list_items_are_prefixed_with_a_bullet()
    {
        var body = ExportAndReopen("- First\n- Second\n");

        var paragraphs = body.Elements<Paragraph>().ToList();
        Assert.Contains(paragraphs, p => p.InnerText == "• First");
        Assert.Contains(paragraphs, p => p.InnerText == "• Second");
    }

    [Fact]
    public void Numbered_list_items_increment()
    {
        var body = ExportAndReopen("1. First\n2. Second\n3. Third\n");

        var texts = body.Elements<Paragraph>().Select(p => p.InnerText).ToList();
        Assert.Contains("1. First", texts);
        Assert.Contains("2. Second", texts);
        Assert.Contains("3. Third", texts);
    }

    [Fact]
    public void Task_list_items_use_checkbox_glyphs()
    {
        var body = ExportAndReopen("- [ ] Todo\n- [x] Done\n");

        var texts = body.Elements<Paragraph>().Select(p => p.InnerText).ToList();
        Assert.Contains("☐ Todo", texts);
        Assert.Contains("☑ Done", texts);
    }

    [Fact]
    public void Blockquote_paragraph_is_indented()
    {
        var body = ExportAndReopen("> Quoted text");

        var paragraph = body.Elements<Paragraph>().First(p => p.InnerText.Contains("Quoted text"));
        int indent = int.Parse(paragraph.ParagraphProperties!.Indentation!.Left!.Value!);
        Assert.True(indent > 0);
    }

    [Fact]
    public void Code_block_preserves_all_lines_in_monospace()
    {
        var body = ExportAndReopen("```\nline one\nline two\n```");

        var runs = body.Descendants<Run>().Where(r => r.RunProperties?.RunFonts?.Ascii == "Consolas").ToList();
        Assert.Contains(runs, r => r.InnerText == "line one");
        Assert.Contains(runs, r => r.InnerText == "line two");
    }

    [Fact]
    public void Horizontal_rule_becomes_a_bordered_paragraph()
    {
        var body = ExportAndReopen("Before\n\n---\n\nAfter");

        Assert.Contains(body.Elements<Paragraph>(),
            p => p.ParagraphProperties?.ParagraphBorders?.BottomBorder is not null);
    }

    [Fact]
    public void Table_produces_correct_row_and_cell_counts_with_bold_header()
    {
        const string markdown = "| A | B |\n| --- | --- |\n| 1 | 2 |\n| 3 | 4 |\n";

        var body = ExportAndReopen(markdown);

        var table = body.Elements<Table>().Single();
        var rows = table.Elements<TableRow>().ToList();
        Assert.Equal(3, rows.Count); // header + 2 body rows
        Assert.Equal(2, rows[0].Elements<TableCell>().Count());

        var headerRun = rows[0].Descendants<Run>().First(r => r.InnerText == "A");
        Assert.NotNull(headerRun.RunProperties?.Bold);

        var bodyRun = rows[1].Descendants<Run>().First(r => r.InnerText == "1");
        Assert.Null(bodyRun.RunProperties?.Bold);
    }

    [Fact]
    public void Link_becomes_a_hyperlink_with_a_relationship_to_the_url()
    {
        using var stream = new MemoryStream();
        DocxExporter.Export("See [our site](https://example.com/page).", stream);

        stream.Position = 0;
        using var doc = WordprocessingDocument.Open(stream, false);
        var mainPart = doc.MainDocumentPart!;
        var hyperlink = mainPart.Document!.Body!.Descendants<Hyperlink>().Single();

        var relationship = mainPart.HyperlinkRelationships.First(r => r.Id == hyperlink.Id);
        Assert.Equal("https://example.com/page", relationship.Uri.ToString());
        Assert.Equal("our site", hyperlink.InnerText);
    }

    [Fact]
    public void Image_becomes_a_placeholder_instead_of_being_embedded()
    {
        var body = ExportAndReopen("![a diagram](diagram.png)");

        Assert.Contains("[Image: a diagram]", body.InnerText);
    }

    [Fact]
    public void Nested_list_items_are_indented_further_than_their_parent()
    {
        const string markdown = "- Parent\n  - Child\n";

        var body = ExportAndReopen(markdown);

        var paragraphs = body.Elements<Paragraph>().ToList();
        var parent = paragraphs.First(p => p.InnerText == "• Parent");
        var child = paragraphs.First(p => p.InnerText == "• Child");

        int parentIndent = int.Parse(parent.ParagraphProperties!.Indentation!.Left!.Value!);
        int childIndent = int.Parse(child.ParagraphProperties!.Indentation!.Left!.Value!);
        Assert.True(childIndent > parentIndent);
    }

    [Fact]
    public void Produces_a_valid_docx_that_reopens_without_error_for_a_full_sample_document()
    {
        const string markdown = """
            # Sample

            Some **bold**, *italic*, ~~struck~~, and `code` text with a [link](https://example.com).

            - Bullet one
            - [x] Done task

            1. First
            2. Second

            > A quote

            ```
            code block
            ```

            | Col A | Col B |
            | --- | --- |
            | 1 | 2 |

            ---

            ![alt](image.png)
            """;

        var body = ExportAndReopen(markdown);

        Assert.NotEmpty(body.InnerText);
    }
}
