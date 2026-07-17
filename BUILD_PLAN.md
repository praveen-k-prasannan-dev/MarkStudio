# Markdown Editor — Complete Build Plan

> **Purpose of this document:** This is the master action plan for Claude (or any developer) to build a
> Windows desktop **Markdown document viewer & editor** with a **Microsoft Word–style toolbox (ribbon)**
> and **PDF export**. Work through the phases **in order**. Each phase ends with a working, runnable
> application and passing tests. Check off tasks (`[x]`) as they are completed.

---

## 1. Product Overview

A desktop application for Windows that lets a user:

- Open, view, edit, and save Markdown (`.md`) files.
- Edit using a **Word-like ribbon toolbar** — the user clicks buttons (Bold, Heading, Table, List…)
  instead of memorizing Markdown syntax.
- See a **live rendered preview** side-by-side with the source (split view), with synchronized scrolling.
- **Export to PDF** (and standalone HTML) with page setup options.
- Work comfortably: recent files, unsaved-change protection, find & replace, word count, themes.

**Non-goals (v1):** collaboration/multi-user editing, cloud sync, plugins, non-Windows platforms.

---

## 2. Technology Stack (fixed decisions — do not re-litigate)

| Concern            | Choice                                   | Why |
|--------------------|------------------------------------------|-----|
| UI framework       | **WPF on .NET 8** (`net8.0-windows`)     | Mature Windows desktop UI, user is a C# developer |
| MVVM helper        | **CommunityToolkit.Mvvm**                | Source-generated observable properties & commands |
| Markdown engine    | **Markdig** (with advanced extensions)   | Best .NET Markdown parser; tables, task lists, footnotes |
| Source editor pane | **AvalonEdit**                           | Proven editor control; syntax highlighting, undo/redo |
| Preview pane       | **WebView2** (`Microsoft.Web.WebView2`)  | Chromium-quality HTML rendering |
| PDF export         | **WebView2 `PrintToPdfAsync`**           | Free, built-in, pixel-perfect vs. preview |
| Unit tests         | **xUnit** + **FluentAssertions**         | Standard .NET testing stack |
| Architecture       | MVVM; all logic in a UI-free **Core** library so it is unit-testable |

**Key architectural rule:** anything that can be tested without a window — Markdown→HTML conversion,
formatting/insertion logic, table generation, document state, file service — lives in
`MarkdownEditor.Core` (plain `net8.0` class library). The WPF project stays a thin shell.

---

## 3. Solution Structure

```
MarkDownProject/
├── BUILD_PLAN.md                  ← this file
├── MarkdownEditor.sln
├── src/
│   ├── MarkdownEditor.Core/       ← net8.0 class library (NO WPF references)
│   │   ├── Markdown/
│   │   │   ├── MarkdownRenderer.cs        # Markdig pipeline → HTML (with CSS theme)
│   │   │   └── HtmlDocumentBuilder.cs     # wraps body HTML into a full styled page
│   │   ├── Editing/
│   │   │   ├── TextSelection.cs           # (Text, SelectionStart, SelectionLength) record
│   │   │   ├── EditResult.cs              # (NewText, NewSelectionStart, NewSelectionLength)
│   │   │   ├── InlineFormatter.cs         # bold/italic/strikethrough/code/highlight toggle
│   │   │   ├── BlockFormatter.cs          # headings, quotes, code fences, lists, HR
│   │   │   ├── ListFormatter.cs           # bullet / numbered / task list toggling
│   │   │   └── TableBuilder.cs            # generate + manipulate Markdown tables
│   │   ├── Documents/
│   │   │   ├── DocumentState.cs           # path, text, IsDirty, title
│   │   │   └── DocumentStatistics.cs      # word/char/line count
│   │   └── Services/
│   │       ├── IFileService.cs / FileService.cs      # load/save with encoding handling
│   │       └── IRecentFilesService.cs / RecentFilesService.cs
│   └── MarkdownEditor.App/        ← net8.0-windows WPF app
│       ├── App.xaml / App.xaml.cs
│       ├── MainWindow.xaml        # ribbon + split view + status bar
│       ├── ViewModels/
│       │   ├── MainViewModel.cs
│       │   └── ExportPdfViewModel.cs
│       ├── Views/
│       │   ├── InsertLinkDialog.xaml
│       │   ├── InsertImageDialog.xaml
│       │   ├── InsertTableDialog.xaml     # Word-style row×column grid picker
│       │   ├── FindReplacePanel.xaml
│       │   └── ExportPdfDialog.xaml
│       ├── Controls/
│       │   └── EditorControl.cs           # AvalonEdit wrapper exposing TextSelection
│       ├── Services/
│       │   ├── PdfExportService.cs        # off-screen WebView2 → PrintToPdfAsync
│       │   └── DialogService.cs
│       └── Assets/
│           ├── preview-light.css / preview-dark.css   # GitHub-like preview styling
│           └── Icons/                     # toolbar icons (use Segoe MDL2 glyphs first)
└── tests/
    └── MarkdownEditor.Core.Tests/ ← xUnit test project (references Core only)
        ├── MarkdownRendererTests.cs
        ├── InlineFormatterTests.cs
        ├── BlockFormatterTests.cs
        ├── ListFormatterTests.cs
        ├── TableBuilderTests.cs
        ├── DocumentStateTests.cs
        ├── DocumentStatisticsTests.cs
        └── RecentFilesServiceTests.cs
```

---

## 4. The Word-Style Toolbox (Ribbon) — Full Specification

Use a **tabbed ribbon-style ToolBar** built from standard WPF controls (`TabControl` + `ToolBar`s —
do **not** take a third-party ribbon dependency). Buttons show an icon + tooltip with keyboard shortcut.
Every button works on the **current selection** in the editor via the Core formatters.

### Tab: Home
| Group      | Controls | Behavior (Markdown produced) |
|------------|----------|------------------------------|
| Clipboard  | Cut, Copy, Paste, Undo, Redo | standard editor commands |
| Font       | **Bold** (Ctrl+B), *Italic* (Ctrl+I), ~~Strikethrough~~, `Inline code`, ==Highlight== | toggles `**x**`, `*x*`, `~~x~~`, `` `x` ``, `==x==` around selection |
| Paragraph  | Heading dropdown (Normal, H1–H6, Ctrl+1…6), Bullet list, Numbered list, Task list, Blockquote, Increase/Decrease indent | toggles `#`…`######`, `- `, `1. `, `- [ ] `, `> ` line prefixes |
| Editing    | Find (Ctrl+F), Replace (Ctrl+H), Select All | opens Find/Replace panel |

### Tab: Insert
| Group   | Controls | Behavior |
|---------|----------|----------|
| Tables  | **Table grid picker** (hover a grid to pick rows×columns, like Word) + "Insert Table…" dialog | inserts a well-formed pipe table with header row |
| Links   | Link (Ctrl+K), Image | dialogs producing `[text](url)` / `![alt](path)` |
| Blocks  | Code block (with language dropdown: csharp, js, python, json, sql, bash, none), Horizontal rule, Footnote | fenced ``` block, `---`, `[^1]` |
| Symbols | Date/time stamp, Emoji picker (small curated set) | inserts text |

### Tab: View
| Group  | Controls |
|--------|----------|
| Layout | Split view / Editor only / Preview only (radio buttons) |
| Sync   | Synchronized scrolling toggle |
| Theme  | Light / Dark preview theme; editor font size +/- |
| Panels | Document outline (headings tree; click → jump to heading) |

### Tab: Export
| Group  | Controls |
|--------|----------|
| Export | **Export to PDF…** (opens page-setup dialog), Export to HTML, Print (Ctrl+P) |

**Toggle behavior contract (must be honored by Core formatters and covered by tests):**
- Applying a format to already-formatted text **removes** it (toggle), e.g. Bold on `**abc**` → `abc`.
- Applying with empty selection inserts the markers and places the caret between them.
- Heading commands replace any existing heading level on the line (H2 applied to an H4 line → H2).
- List commands operate on **all selected lines**; mixed lists normalize to the requested type.

---

## 5. Build Phases — Step-by-Step Actions

### Phase 0 — Solution scaffolding
- [x] 0.1 `git init` in the working folder; add a .NET-appropriate `.gitignore`.
- [x] 0.2 Create solution + projects:
  ```powershell
  dotnet new sln -n MarkdownEditor
  dotnet new classlib -n MarkdownEditor.Core -o src/MarkdownEditor.Core -f net8.0
  dotnet new wpf      -n MarkdownEditor.App  -o src/MarkdownEditor.App  -f net8.0
  dotnet new xunit    -n MarkdownEditor.Core.Tests -o tests/MarkdownEditor.Core.Tests -f net8.0
  dotnet sln add (Get-ChildItem -Recurse *.csproj).FullName
  ```
- [x] 0.3 Add references: App → Core; Tests → Core.
- [x] 0.4 Add NuGet packages:
  - Core: `Markdig`
  - App: `CommunityToolkit.Mvvm`, `AvalonEdit`, `Microsoft.Web.WebView2`
  - Tests: `FluentAssertions` **7.2.2** (pinned — v8+ requires a paid commercial license)
- [x] 0.5 `dotnet build` and `dotnet test` must pass (empty test OK). **Commit.**
  - Note: SDK 10 generated `MarkdownEditor.slnx` (new solution format) instead of `.sln`; a
    project-level `nuget.config` pins restore to nuget.org (corporate feed was returning 503s).

### Phase 1 — Core: Markdown rendering (TDD)
- [x] 1.1 Write `MarkdownRendererTests` first (see §6 test list), then implement `MarkdownRenderer`:
      Markdig pipeline with `UseAdvancedExtensions()` (already includes emphasis extras, task
      lists, and pipe tables); method `string ToHtml(string markdown)`.
- [x] 1.2 Implement `HtmlDocumentBuilder.BuildPage(bodyHtml, cssText, title)` → full `<html>` document.
- [x] 1.3 Create `preview-light.css` (GitHub-like: fonts, table borders, code blocks, blockquote bar)
      and `preview-dark.css`. Copied to output via csproj `Content` item.
- [x] 1.4 All Phase 1 tests green (22 tests). **Commit.**

### Phase 2 — Core: Editing engine (TDD — this powers the whole toolbox)
- [x] 2.1 Define `TextSelection` and `EditResult` records.
- [x] 2.2 `InlineFormatter.Toggle(selection, marker)` for `**`, `*`, `~~`, `` ` ``, `==`.
      Handle: empty selection, already-wrapped selection (unwrap), selection with surrounding
      whitespace (trim markers inside), multi-word selection.
- [x] 2.3 `BlockFormatter`: `SetHeading(selection, level 0–6)` (0 = remove), `ToggleBlockquote`,
      `ToggleCodeFence(language)`, `InsertHorizontalRule` (with setext-heading protection).
- [x] 2.4 `ListFormatter.ToggleList(selection, ListKind.Bullet|Numbered|Task)` across all selected
      lines; renumber numbered lists; convert between kinds.
- [x] 2.5 `TableBuilder.Create(rows, cols, hasHeader)` → aligned pipe table;
      `InsertRow/InsertColumn(tableText, index)` for later table editing.
- [x] 2.6 `DocumentStatistics.Compute(text)` → words, characters, lines (ignore Markdown syntax
      markers for the word count). Also implemented `DocumentState`, `FileService`,
      `RecentFilesService` (were listed under §3 structure).
- [x] 2.7 All Phase 2 tests green (72 tests total). **Commit.**

### Phase 3 — App shell: window, editor, preview
- [x] 3.1 `MainWindow` layout: File menu + ribbon area placeholder on top, `Grid` with AvalonEdit
      (left) + `GridSplitter` + WebView2 (right), `StatusBar` at bottom
      (word/char/line count, caret line:col; dirty marker `●` shown in window title).
- [x] 3.2 Editor wiring in code-behind (AvalonEdit's built-in "MarkDown" highlighting definition;
      dedicated `EditorControl` deferred — formatters are wired directly in Phase 4).
- [x] 3.3 `MainViewModel`: `DocumentState`, dirty tracking, title = `name ● — Markdown Editor`,
      status-bar text; UI-framework-free.
- [x] 3.4 Live preview: 300 ms debounce, render via Core, `NavigateToString`; scroll position
      preserved across refreshes; document folder mapped to `https://preview.local/` virtual host
      so relative image paths work.
- [x] 3.5 File handling: New / Open / Save / Save As (Ctrl+N/O/S/Shift+S), unsaved-changes prompt on
      close/new/open, recent files (persist to `%APPDATA%\MarkdownEditor\recent.json`),
      file-drop onto window opens the file, `.md` passed as command-line arg opens on startup.
- [x] 3.6 Verified: app builds, launches, and stays running; all 72 tests green. **Commit.**

### Phase 4 — The Word-style toolbox (wire ribbon → Core formatters)
- [x] 4.1 Build the ribbon per §4 spec: Home, Insert, View, Export tabs (text/emoji button faces
      instead of MDL2 glyphs — safer rendering; buttons are `Focusable=False` so editor selection
      is preserved).
- [x] 4.2 Bind every formatting button to routed commands handled in `MainWindow.Ribbon.cs`: get
      editor selection → Core formatter → apply `EditResult` as a minimal single-undo-step
      replacement → refocus editor.
- [x] 4.3 Keyboard shortcuts: Ctrl+B/I/K, Ctrl+1…6/0 (headings), Ctrl+Shift+7/8/9 (lists),
      Ctrl+Shift+X/C/H/Q/K, Ctrl+F/H, F3/Shift+F3 (see `EditorCommands.cs`).
- [x] 4.4 Insert dialogs: Link, Image (file picker, auto-relative path), Table (hover grid picker
      **and** rows/cols dialog), code-block language dropdown, footnote, date, emoji menu.
- [x] 4.5 Find & Replace panel (find next/prev with wrap-around, replace one/all, match case).
- [x] 4.6 View tab: split/editor/preview modes, light/dark preview theme switch, sync-scroll toggle,
      editor font size, outline panel (heading regex; click navigates editor).
- [x] 4.7 Synchronized scrolling: proportional editor→preview scroll mapping (fraction of scroll
      height; the `data-line` element mapping remains a future enhancement).
- [ ] 4.8 Manual verification of **every** toolbox button (user pass pending). **Commit.**

### Phase 5 — Export: PDF, HTML, print
- [x] 5.1 PDF export reuses the preview WebView2 (simpler than an off-screen instance): re-render
      with the light theme, await navigation, `PrintToPdfAsync` with `CoreWebView2PrintSettings`,
      then restore the live preview.
- [x] 5.2 `ExportPdfDialog`: page size (A4/Letter), orientation, margins (narrow/normal/wide),
      include background graphics toggle; remembers last-used settings (per session).
- [x] 5.3 Export to HTML: write the same full page (inline CSS) to a chosen `.html` file.
- [x] 5.4 Print: `CoreWebView2.ShowPrintUI()` on the preview (Ctrl+P).
- [ ] 5.5 Verify: export a document containing headings, a table, a task list, a code block, and an
      image; open the PDF and confirm fidelity (user pass pending). **Commit.**

### Phase 6 — Polish & hardening
- [ ] 6.1 Auto-save recovery draft every 60 s to `%APPDATA%\MarkdownEditor\autosave\`; offer recovery
      after a crash.
- [ ] 6.2 Settings persistence (`settings.json`): theme, view mode, font size, window size/position.
- [ ] 6.3 Word count in status bar updates live (Core `DocumentStatistics`).
- [ ] 6.4 Drag-and-drop an image file into the editor → copies it to `./assets/` next to the `.md`
      and inserts `![name](assets/name.png)`.
- [ ] 6.5 Error handling: WebView2 runtime missing → friendly message with download link; file I/O
      errors → non-crashing dialogs.
- [ ] 6.6 App icon, version info, About dialog.
- [ ] 6.7 Full regression pass: all tests green + manual checklist (§7). **Commit.**

---

## 6. Unit Test Plan (xUnit + FluentAssertions, TDD for Core)

> Write these tests **before or alongside** the Core implementation. Target: **≥ 80% line coverage
> on `MarkdownEditor.Core`**. Run with `dotnet test`. UI (WPF) is verified manually via §7.

### MarkdownRendererTests
- Heading `# Title` → `<h1>` containing `Title` (repeat for H2–H6).
- `**bold**` → `<strong>`; `*italic*` → `<em>`; `~~x~~` → `<del>`; `==x==` → `<mark>`.
- Pipe table renders `<table>` with correct header cells and row count.
- Task list `- [x] done` → checkbox input checked; `- [ ]` unchecked.
- Fenced code block with language → `<pre><code class="language-csharp">`; code content is HTML-escaped.
- Link and image produce correct `href`/`src`; raw HTML `<script>` in Markdown is left inert/escaped per pipeline config (document the chosen behavior).
- Empty string → empty/blank HTML without throwing; `null` → `ArgumentNullException`.

### InlineFormatterTests
- Wrap: `abc` selected → `**abc**`, selection covers `abc` inside markers.
- Toggle off: selection `**abc**` (or `abc` inside markers) → `abc`.
- Empty selection: inserts `****`, caret lands between the markers.
- Selection with trailing space `"abc "` → `**abc** ` (markers hug the word).
- Each marker type: `*`, `**`, `~~`, `` ` ``, `==`.

### BlockFormatterTests
- SetHeading(2) on plain line `Hello` → `## Hello`.
- SetHeading(2) on `#### Hello` → `## Hello` (replaces level).
- SetHeading(0) on `## Hello` → `Hello` (removes).
- Multi-line selection: heading applies to every selected line.
- ToggleBlockquote adds/removes `> ` on all selected lines.
- ToggleCodeFence wraps selection in ```` ```lang … ``` ```` and unwraps if already fenced.

### ListFormatterTests
- Toggle bullet on 3 plain lines → each prefixed `- `; toggle again → prefixes removed.
- Toggle numbered on 3 lines → `1.`, `2.`, `3.`.
- Convert bullet list to task list → `- [ ] ` items (preserving text).
- Numbered list renumbers after conversion; blank lines within selection are left untouched.

### TableBuilderTests
- Create(2,3,header) → header row + separator `| --- |`×3 + 2 body rows; column counts consistent on every line.
- Cell padding aligned (columns same width) — assert well-formed pipe structure.
- InsertRow / InsertColumn produce a still-valid table with expected dimensions.

### DocumentStateTests
- New document: `IsDirty == false`, title "Untitled".
- Text change sets `IsDirty`; save resets it; loading a file sets path + clean state.

### DocumentStatisticsTests
- `"Hello world"` → 2 words; empty → 0; Markdown markers (`#`, `**`) not counted as words.

### RecentFilesServiceTests
- Add pushes to front; duplicates move to front (no dupes); capped at 10; persists and reloads
  via an injected in-memory/temp-path store.

---

## 7. Manual UI Verification Checklist (run after Phases 4–6)

- [ ] Every ribbon button produces correct Markdown and the preview updates.
- [ ] Every keyboard shortcut works and matches its tooltip.
- [ ] Bold toggle works: apply → appears; apply again → removed.
- [ ] Table grid picker inserts exactly the hovered dimensions.
- [ ] Open → edit → Save round-trip preserves content byte-for-byte apart from edits (UTF-8).
- [ ] Closing with unsaved changes prompts; Cancel really cancels.
- [ ] Split/editor-only/preview-only modes; sync-scroll follows caret.
- [ ] PDF export of the sample document (headings, table, task list, code, image) looks correct.
- [ ] Dark theme preview readable; theme choice persists across restart.
- [ ] App does not freeze while typing rapidly in a large (1 MB) document.

---

## 8. Working Agreements for Claude

1. **One phase at a time.** Finish the phase, run `dotnet build` and `dotnet test`, tick the
   checkboxes in this file, commit with message `Phase N: <summary>`, then continue.
2. **Core stays UI-free.** If a piece of logic needs `System.Windows.*`, it belongs in the App
   project; otherwise put it in Core and test it.
3. **TDD for Core:** for Phases 1–2, write the failing test first.
4. **Don't add packages** beyond §2 without noting the reason in this file.
5. **Keep this file updated** — it is the single source of truth for progress.
