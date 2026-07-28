# Spell Checking

Misspelled words get a dotted red underline as you type, similar to Word.

- **Right-click** a misspelled word for up to five suggested corrections at the top of the [context menu](context-menu.md) — click one to replace the word.
- **Add to Dictionary** stops that word being flagged again — useful for names, product terms, or anything technical you use often.
- Spell checking uses an offline Hunspell dictionary (`en_US`) bundled with the app — no network access involved.

## What's excluded

Code blocks, inline code, and URLs are never spell-checked, since flagging `getElementById` or a web address as "misspelled" would just be noise.
