# Autosave & Crash Recovery

While you have unsaved changes, MarkStudio Editor writes a recovery draft to disk every 60 seconds. This covers the active tab, so if you're working across multiple tabs, the most recently active one is the one protected.

If the application is ever terminated abnormally — a crash, a power loss, a forced shutdown — the **next time you launch the app**, it detects the leftover draft and asks:

> **Recover document** — Unsaved changes from a previous session were found. Do you want to restore them?

Choose **Yes** to load the recovered text into a new tab, or **No** to discard it. Once you save normally, the recovery draft is deleted, since the file on disk is now up to date.

## Where it's stored

The recovery draft and your other local app data live under:

```
%APPDATA%\MarkdownEditor\
```

This includes the autosave draft, your settings (theme, window size, recent files), and your [Writing Stats](../ribbon/writing-stats.md) history.
