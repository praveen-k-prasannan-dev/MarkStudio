using MarkdownEditor.Core.Documents;

namespace MarkdownEditor.Core.Tests;

public class DocumentTemplatesTests
{
    [Fact]
    public void Built_in_list_is_not_empty()
    {
        Assert.NotEmpty(DocumentTemplates.BuiltIn);
    }

    [Fact]
    public void Every_template_has_a_name_description_and_non_empty_content()
    {
        foreach (var template in DocumentTemplates.BuiltIn)
        {
            Assert.False(string.IsNullOrWhiteSpace(template.Name));
            Assert.False(string.IsNullOrWhiteSpace(template.Description));
            Assert.False(string.IsNullOrWhiteSpace(template.Content));
        }
    }

    [Fact]
    public void Template_names_are_unique()
    {
        var names = DocumentTemplates.BuiltIn.Select(t => t.Name).ToList();

        Assert.Equal(names.Count, names.Distinct().Count());
    }

    [Fact]
    public void Includes_meeting_notes_and_readme_templates()
    {
        Assert.Contains(DocumentTemplates.BuiltIn, t => t.Name == "Meeting Notes");
        Assert.Contains(DocumentTemplates.BuiltIn, t => t.Name == "README");
    }
}
