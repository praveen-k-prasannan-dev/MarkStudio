using FluentAssertions;
using MarkdownEditor.Core.Documents;

namespace MarkdownEditor.Core.Tests;

public class DocumentStateTests
{
    [Fact]
    public void New_document_is_clean_and_untitled()
    {
        var doc = new DocumentState();

        doc.IsDirty.Should().BeFalse();
        doc.Title.Should().Be("Untitled");
        doc.FilePath.Should().BeNull();
        doc.Text.Should().BeEmpty();
    }

    [Fact]
    public void Changing_text_marks_document_dirty()
    {
        var doc = new DocumentState();

        doc.SetText("# Hi");

        doc.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void Setting_identical_text_does_not_mark_dirty()
    {
        var doc = new DocumentState();

        doc.SetText("");

        doc.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void MarkSaved_resets_dirty_and_updates_title()
    {
        var doc = new DocumentState();
        doc.SetText("content");

        doc.MarkSaved(@"C:\docs\notes.md");

        doc.IsDirty.Should().BeFalse();
        doc.Title.Should().Be("notes.md");
    }

    [Fact]
    public void LoadFrom_sets_path_text_and_clean_state()
    {
        var doc = new DocumentState();

        doc.LoadFrom(@"C:\docs\readme.md", "# Readme");

        doc.Text.Should().Be("# Readme");
        doc.IsDirty.Should().BeFalse();
        doc.Title.Should().Be("readme.md");
    }

    [Fact]
    public void Reset_returns_to_untitled_clean_state()
    {
        var doc = new DocumentState();
        doc.LoadFrom(@"C:\docs\readme.md", "# Readme");

        doc.Reset();

        doc.FilePath.Should().BeNull();
        doc.Text.Should().BeEmpty();
        doc.IsDirty.Should().BeFalse();
    }
}
