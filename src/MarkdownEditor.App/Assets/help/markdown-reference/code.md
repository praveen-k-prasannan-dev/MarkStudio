# Code (Inline & Fenced)

## Inline code

```
Use `getElementById` to find it.
```

Use `getElementById` to find it.

## Fenced code blocks

Wrap a block in triple backticks, optionally followed by a language name for syntax highlighting in the preview:

````
```csharp
var renderer = new MarkdownRenderer();
string html = renderer.ToHtml("# Hello");
```
````

renders as:

```csharp
var renderer = new MarkdownRenderer();
string html = renderer.ToHtml("# Hello");
```

The ribbon's **Code Block** button (`Ctrl+Shift+K`, [Insert Tab — Blocks](../ribbon/insert-blocks.md)) lets you pick from common languages (`csharp`, `python`, `javascript`, `typescript`, `sql`, `bash`, `xml`, `html`, `css`, `yaml`, `json`) — or type `code` and press `Tab` for a quick unlabeled block ([Templates & Snippets](../templates-snippets.md)).

A fence labeled ` ```mermaid ` is special-cased to render as an actual diagram instead of highlighted text — see [Mermaid Diagrams](../mermaid/overview.md).

Neither inline code nor fenced code blocks are ever flagged by [spell check](../spell-checking.md).
