# Tips & Troubleshooting

## "The preview pane is disabled" / WebView2 warnings

The live preview, PDF export, and this Help window are all rendered using the **Microsoft Edge WebView2 Runtime**. It's already installed on Windows 11 and on any PC with Microsoft Edge, so most people never see this. If it's missing, the app shows a message with a free download link — install it and restart the app.

## Windows SmartScreen warning on first run

If you downloaded the `.zip` release directly (rather than via the Microsoft Store), the executable isn't code-signed, so Windows may show *"Windows protected your PC."* Click **More info → Run anyway**. The Microsoft Store version doesn't show this warning, since the Store signs the package.

## Where your data lives

| What | Location |
|------|----------|
| Settings (theme, window size, recent files) | `%APPDATA%\MarkdownEditor\settings.json` |
| Autosave/crash-recovery draft | `%APPDATA%\MarkdownEditor\autosave\recovery.md` |
| Writing stats history | `%APPDATA%\MarkdownEditor\writing-stats.json` |
| Diagnostic log | `%APPDATA%\MarkdownEditor\app.log` |

Nothing here is ever sent anywhere — MarkStudio Editor makes no network calls of its own. Mermaid and MathJax are bundled locally rather than loaded from a CDN for the same reason.

## A Mermaid diagram or math formula isn't rendering

- Check the fence/delimiter syntax exactly matches what's shown in [Mermaid Diagrams](mermaid/overview.md) or [Math Formulas](math/overview.md) — a missing closing ` ``` ` or `$` is the most common cause.
- Both libraries only load when a document actually contains a diagram or formula, so the very first render after adding one can take a moment longer than usual.

## Recovering unsaved work after a crash

See [Autosave & Crash Recovery](getting-started/autosave.md).
