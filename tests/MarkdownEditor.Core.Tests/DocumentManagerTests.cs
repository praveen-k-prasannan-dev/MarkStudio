using FluentAssertions;
using MarkdownEditor.Core.Documents;

namespace MarkdownEditor.Core.Tests;

public class DocumentManagerTests
{
    [Fact]
    public void Empty_manager_has_no_active_document()
    {
        var manager = new DocumentManager();

        manager.Active.Should().BeNull();
        manager.ActiveIndex.Should().Be(-1);
        manager.Count.Should().Be(0);
    }

    [Fact]
    public void AddNew_becomes_active_and_is_titled_Untitled()
    {
        var manager = new DocumentManager();

        var doc = manager.AddNew();

        manager.Active.Should().BeSameAs(doc);
        doc.Title.Should().Be("Untitled");
    }

    [Fact]
    public void Second_AddNew_is_numbered_and_becomes_active()
    {
        var manager = new DocumentManager();
        var first = manager.AddNew();

        var second = manager.AddNew();

        first.Title.Should().Be("Untitled");
        second.Title.Should().Be("Untitled 2");
        manager.Active.Should().BeSameAs(second);
    }

    [Fact]
    public void Untitled_numbers_are_never_reused_after_closing()
    {
        var manager = new DocumentManager();
        manager.AddNew();               // Untitled
        manager.AddNew();               // Untitled 2
        manager.Close(0);                // close "Untitled"

        var third = manager.AddNew();

        third.Title.Should().Be("Untitled 3");
    }

    [Fact]
    public void SetActive_by_index_switches_the_active_document()
    {
        var manager = new DocumentManager();
        var first = manager.AddNew();
        manager.AddNew();

        manager.SetActive(0);

        manager.Active.Should().BeSameAs(first);
    }

    [Fact]
    public void SetActive_by_reference_switches_the_active_document()
    {
        var manager = new DocumentManager();
        var first = manager.AddNew();
        manager.AddNew();

        manager.SetActive(first);

        manager.ActiveIndex.Should().Be(0);
    }

    [Fact]
    public void SetActive_with_a_foreign_document_throws()
    {
        var manager = new DocumentManager();
        manager.AddNew();
        var foreign = new DocumentState();

        var act = () => manager.SetActive(foreign);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void FindByPath_is_case_insensitive_and_ignores_unsaved_documents()
    {
        var manager = new DocumentManager();
        var unsaved = manager.AddNew();
        var saved = manager.AddNew();
        saved.LoadFrom(@"C:\Notes\readme.MD", "content");

        manager.FindByPath(@"c:\notes\readme.md").Should().BeSameAs(saved);
        manager.FindByPath(@"C:\Notes\missing.md").Should().BeNull();
    }

    [Fact]
    public void Close_removes_the_document_and_returns_the_new_active_index()
    {
        var manager = new DocumentManager();
        manager.AddNew();
        manager.AddNew();
        manager.AddNew(); // active index 2

        int newActive = manager.Close(2);

        newActive.Should().Be(1);
        manager.Count.Should().Be(2);
    }

    [Fact]
    public void Closing_a_tab_before_the_active_one_shifts_the_active_index_down()
    {
        var manager = new DocumentManager();
        manager.AddNew();
        manager.AddNew();
        var active = manager.AddNew(); // index 2, active

        manager.Close(0);

        manager.ActiveIndex.Should().Be(1);
        manager.Active.Should().BeSameAs(active);
    }

    [Fact]
    public void Closing_the_last_remaining_document_leaves_none_active()
    {
        var manager = new DocumentManager();
        manager.AddNew();

        int newActive = manager.Close(0);

        newActive.Should().Be(-1);
        manager.Active.Should().BeNull();
        manager.Count.Should().Be(0);
    }

    [Fact]
    public void Closing_the_active_middle_tab_selects_the_next_one()
    {
        var manager = new DocumentManager();
        manager.AddNew();
        var second = manager.AddNew();
        manager.AddNew();
        manager.SetActive(1); // "second" is active

        manager.Close(1);

        manager.Active.Should().NotBeSameAs(second);
        manager.ActiveIndex.Should().Be(1); // the tab that shifted into index 1
    }
}
