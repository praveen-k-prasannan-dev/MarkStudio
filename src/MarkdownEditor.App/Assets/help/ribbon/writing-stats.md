# Writing Stats

View tab → **Stats** opens a small popup showing:

- **Today** — how many words you've written today
- **Last 7 days** — a trailing 7-day total
- **Streak** — how many consecutive days you've written something

Click **Stats** again to close the popup.

## How "words written" is counted

Stats only count net *increases* in word count, sampled periodically as you type. Practically, this means:

- Typing new content always adds to today's total.
- Deleting text, or undoing, never subtracts from it — your stats only ever go up.
- If you delete a large chunk and then type it back, that re-typing counts again (since it's measured as new content appearing, not tracked per-character).

This approximates writing *effort* rather than the net length of your document, which is deliberately different once you start editing/revising rather than just drafting.

Stats are saved to disk (`%APPDATA%\MarkdownEditor\writing-stats.json`) and persist across restarts, so your streak survives closing the app.
