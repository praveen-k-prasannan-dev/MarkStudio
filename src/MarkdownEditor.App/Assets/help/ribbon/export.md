# Export Tab

| Button | Shortcut | Output |
|--------|----------|--------|
| 📄 Export to PDF… | — | A PDF, rendered by the same engine as the live preview |
| 📝 Export to Word… | — | A native `.docx` file |
| 🌐 Export to HTML… | — | A standalone HTML file |
| 🖨 Print… | `Ctrl+P` | Prints (or "Print to PDF" via the system print dialog) |

## Export to PDF

Opens a small dialog for page setup — page size (A4/Letter), orientation, margins, and whether to print background colors — then saves a PDF. Since it's rendered by the exact same engine as the preview pane, **what you see is exactly what you get**.

## Export to Word

Builds a native `.docx` file directly from your document's Markdown structure (not by converting the HTML preview). Headings, **bold**/*italic*/~~strikethrough~~/`code`, links, bullet/numbered/task lists, blockquotes, code blocks, and tables all map onto real Word formatting.

Two deliberate limitations worth knowing: images become a `[Image: alt text]` placeholder rather than being embedded, and lists render as literal bullet/number/checkbox text rather than Word's native auto-numbering (so reordering list items in Word won't renumber them automatically).

## Export to HTML

Saves a standalone `.html` file with the rendered content and styling inlined, viewable in any browser without needing MarkStudio Editor.

## Print

Opens the system print dialog (or "Microsoft Print to PDF" if you don't have a physical printer), using the same rendering as the preview.
