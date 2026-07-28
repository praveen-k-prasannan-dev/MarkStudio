using System.Text.Json;
using System.Text.Json.Serialization;

namespace MarkdownEditor.Core.Help;

/// <summary>Parses the Help window's table-of-contents JSON (<c>Assets/help/toc.json</c>) into a <see cref="HelpTopic"/> tree.</summary>
public static class HelpTocLoader
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public static IReadOnlyList<HelpTopic> Parse(string json)
    {
        var nodes = JsonSerializer.Deserialize<List<TocNode>>(json, Options) ?? [];
        return nodes.Select(ToTopic).ToList();
    }

    private static HelpTopic ToTopic(TocNode node) => new()
    {
        Title = node.Title,
        File = node.File,
        Children = (node.Children ?? []).Select(ToTopic).ToList(),
    };

    private sealed class TocNode
    {
        public string Title { get; set; } = "";
        public string? File { get; set; }
        [JsonPropertyName("children")]
        public List<TocNode>? Children { get; set; }
    }
}
