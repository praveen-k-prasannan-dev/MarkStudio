# Workspace Mode

**File → Open Folder…** (or View tab → Workspace) scans a folder into a sidebar file tree containing just its Markdown files (`.md`/`.markdown`). Noise folders like `.git`, `.vs`, `node_modules`, `bin`, and `obj` are skipped automatically, and any folder that contains no Markdown files anywhere beneath it is omitted from the tree entirely.

## Using the sidebar

- Click a file in the tree to open it in a new tab.
- Type in the **search box** above the tree to search every file in the workspace, line by line, as you type.
- Double-click a search result to open that file and jump straight to the matching line.
- Clear the search box to go back to browsing the tree.

## Cross-document links

Once a document is saved to disk, clicking a relative link to another `.md` file in the **preview pane** — e.g. `[see the other notes](other.md)` — opens that file in a new tab instead of navigating the preview away from your rendered document. A link to a `https://` URL opens in your default web browser instead of inside the app.

This works for any saved document, whether or not you've opened its containing folder as a workspace.
