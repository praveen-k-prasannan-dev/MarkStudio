namespace MarkdownEditor.Core.Documents;

/// <summary>A named starting point for a new document, offered via File > New From Template.</summary>
public sealed record DocumentTemplate(string Name, string Description, string Content);

public static class DocumentTemplates
{
    public static IReadOnlyList<DocumentTemplate> BuiltIn { get; } =
    [
        new DocumentTemplate(
            "Meeting Notes",
            "Attendees, agenda, and action items.",
            "# Meeting Notes\n\n**Date:** \n**Attendees:** \n\n## Agenda\n\n- \n\n## Discussion\n\n\n\n## Action Items\n\n- [ ] \n"),

        new DocumentTemplate(
            "README",
            "A standard project README skeleton.",
            "# Project Name\n\nOne-line description of what this project does.\n\n## Installation\n\n```\n\n```\n\n## Usage\n\n```\n\n```\n\n## License\n\n"),

        new DocumentTemplate(
            "Changelog",
            "Keep a Changelog-style version history.",
            "# Changelog\n\n## [Unreleased]\n\n### Added\n\n### Changed\n\n### Fixed\n\n"),

        new DocumentTemplate(
            "Blog Post",
            "Title, intro, and section headings for a post.",
            "# Post Title\n\n*Published: *\n\nIntroductory paragraph.\n\n## Heading\n\n\n\n## Conclusion\n\n"),

        new DocumentTemplate(
            "To-Do List",
            "A simple task list.",
            "# To-Do\n\n- [ ] \n- [ ] \n- [ ] \n"),
    ];
}
