# Opening, Creating, and Saving Documents

## Creating a new document

- **File → New** (`Ctrl+N`) creates a blank "Untitled" tab.
- **File → New From Template…** creates a tab pre-filled with a starting structure (Meeting Notes, README, Changelog, Blog Post, To-Do List). See [Templates & Snippets](../templates-snippets.md).

## Opening a document

- **File → Open…** (`Ctrl+O`) opens a file picker.
- **File → Open Recent** lists the last 10 files you opened.
- Drag a `.md` file onto the window to open it.
- Opening a file that's already open in a tab switches to that tab instead of opening a duplicate.

## Saving

- **Ctrl+S** saves. If the document has never been saved, this behaves like Save As.
- **Ctrl+Shift+S** always prompts for a location (Save As).
- The title bar shows a `●` marker whenever there are unsaved changes, e.g. `notes.md ● — MarkStudio Editor`.
- Closing a tab or the window with unsaved changes always asks *Save / Don't save / Cancel* — you can't lose work by accident.

## Autosave & crash recovery

While a document has unsaved changes, a recovery draft is written to disk every 60 seconds. If the app is ever killed unexpectedly (power loss, crash), the next launch offers to restore that draft. See [Autosave & Recovery](autosave.md) for details.
