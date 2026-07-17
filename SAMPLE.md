# Markdown Editor Sample Document

This file exercises every feature the editor supports — open it and try the ribbon,
then use **Export → Export to PDF…** to check output fidelity.

## Text formatting

**Bold**, *italic*, ~~strikethrough~~, `inline code`, and ==highlighted== text.

## Lists

- Bullet item one
- Bullet item two

1. First step
2. Second step

- [x] Completed task
- [ ] Pending task

## Table

| Feature        | Status | Notes                    |
| -------------- | ------ | ------------------------ |
| Live preview   | ✅     | 300 ms debounce          |
| PDF export     | ✅     | WebView2 PrintToPdfAsync |
| Ribbon toolbox | ✅     | Word-style groups        |

## Code

```csharp
var renderer = new MarkdownRenderer();
string html = renderer.ToHtml("# Hello");
Console.WriteLine(html);
```

> Blockquotes work too — with a footnote for good measure.[^1]

---

[^1]: This is the footnote text.
