# Templates & Snippets

## Templates — starting a whole new document

**File → New From Template…** (or search "template" in the [Command Palette](command-palette.md)) opens a picker with ready-made starting points:

| Template | Contents |
|----------|----------|
| Meeting Notes | Date, Attendees, Agenda, Discussion, Action Items |
| README | Project name, description, Installation, Usage, License |
| Changelog | "Keep a Changelog"-style Unreleased/Added/Changed/Fixed sections |
| Blog Post | Title, publish date, intro, a heading, conclusion |
| To-Do List | A simple checklist |

Pick one (or double-click it) and a new tab opens pre-filled with that structure, ready to fill in.

## Snippets — quick inline expansion

Type one of these trigger words on its own, immediately followed by pressing **Tab**:

| Trigger | Expands to |
|---------|-----------|
| `todo` | `- [ ] ` — a task checkbox, cursor ready to type |
| `table` | A 3-column, 2-row table skeleton, cursor in the first body cell |
| `meeting` | A full Meeting Notes structure (Date/Attendees/Agenda/Action Items) |
| `code` | A fenced code block, cursor inside it |

In every case, the cursor lands exactly where you'd want to start typing — not just at the end of the inserted text.

**Snippets only expand when your caret is *not* inside a table** — if you're inside a table, `Tab` always means "next cell" instead, exactly as it did before snippets existed. So typing `table` and pressing Tab while your caret is already inside an existing table just moves to the next cell.
