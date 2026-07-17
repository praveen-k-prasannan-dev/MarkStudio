using FluentAssertions;
using MarkdownEditor.Core.Services;

namespace MarkdownEditor.Core.Tests;

public sealed class RecentFilesServiceTests : IDisposable
{
    private readonly string _storePath =
        Path.Combine(Path.GetTempPath(), "MarkdownEditorTests", Guid.NewGuid() + ".json");

    public void Dispose()
    {
        if (File.Exists(_storePath))
            File.Delete(_storePath);
    }

    [Fact]
    public void Add_pushes_newest_to_front()
    {
        var service = new RecentFilesService(_storePath);

        service.Add(@"C:\a.md");
        service.Add(@"C:\b.md");

        service.Items.Should().ContainInOrder(@"C:\b.md", @"C:\a.md");
    }

    [Fact]
    public void Adding_duplicate_moves_it_to_front_without_duplicating()
    {
        var service = new RecentFilesService(_storePath);
        service.Add(@"C:\a.md");
        service.Add(@"C:\b.md");

        service.Add(@"C:\A.MD"); // same file, different casing

        service.Items.Should().HaveCount(2);
        service.Items[0].Should().Be(@"C:\A.MD");
    }

    [Fact]
    public void List_is_capped_at_ten_entries()
    {
        var service = new RecentFilesService(_storePath);

        for (int i = 1; i <= 15; i++)
            service.Add($@"C:\file{i}.md");

        service.Items.Should().HaveCount(10);
        service.Items[0].Should().Be(@"C:\file15.md");
    }

    [Fact]
    public void Entries_persist_across_instances()
    {
        new RecentFilesService(_storePath).Add(@"C:\persisted.md");

        var reloaded = new RecentFilesService(_storePath);

        reloaded.Items.Should().ContainSingle().Which.Should().Be(@"C:\persisted.md");
    }

    [Fact]
    public void Corrupt_store_file_is_ignored()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_storePath)!);
        File.WriteAllText(_storePath, "not json {{{");

        var service = new RecentFilesService(_storePath);

        service.Items.Should().BeEmpty();
    }

    [Fact]
    public void Clear_empties_the_list()
    {
        var service = new RecentFilesService(_storePath);
        service.Add(@"C:\a.md");

        service.Clear();

        service.Items.Should().BeEmpty();
    }
}
