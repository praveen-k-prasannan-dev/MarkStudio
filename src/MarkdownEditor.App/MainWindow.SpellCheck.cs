using System.IO;
using System.Windows;
using System.Windows.Controls;
using MarkdownEditor.App.Services;
using MarkdownEditor.Core.Spelling;

namespace MarkdownEditor.App;

/// <summary>
/// Offline spell checking: a dotted-red-underline colorizer kept in sync with the active tab's
/// text, plus spelling suggestions added to the editor's right-click menu. Always on when the
/// bundled en_US dictionary loads successfully; there is no on/off toggle in this version.
/// </summary>
public partial class MainWindow
{
    private SpellChecker? _spellChecker;
    private readonly SpellCheckColorizer _spellCheckColorizer = new();
    private IReadOnlyList<MisspelledSpan> _misspelledSpans = [];

    private void InitializeSpellCheck()
    {
        try
        {
            string dictDir = Path.Combine(AppContext.BaseDirectory, "Assets", "dictionaries");
            string dicPath = Path.Combine(dictDir, "en_US.dic");
            string affPath = Path.Combine(dictDir, "en_US.aff");
            if (File.Exists(dicPath) && File.Exists(affPath))
                _spellChecker = SpellChecker.LoadFromFiles(dicPath, affPath);
        }
        catch (Exception ex)
        {
            // Spell check is a nice-to-have; never let a bad dictionary file break the editor.
            Services.AppLog.Write($"Spell checker failed to load: {ex}");
            _spellChecker = null;
        }

        Editor.TextArea.TextView.LineTransformers.Add(_spellCheckColorizer);
    }

    /// <summary>Re-scans the given text for misspelled words and redraws the underlines.</summary>
    private void RescanSpelling(string text)
    {
        if (_spellChecker is null)
            return;

        _misspelledSpans = SpellCheckScanner.FindMisspelledWords(text, _spellChecker.IsCorrect);
        _spellCheckColorizer.MisspelledSpans = _misspelledSpans;
        Editor.TextArea.TextView.Redraw();
    }

    /// <summary>
    /// Populates (or hides) the "Spelling" section of the right-click menu based on whether the
    /// caret - after AvalonEdit's own click-to-reposition behavior - landed on a flagged word.
    /// </summary>
    private void UpdateSpellingContextMenu()
    {
        SpellingContextMenu.Items.Clear();

        if (_spellChecker is null || _misspelledSpans.Count == 0)
        {
            SpellingContextMenu.Visibility = Visibility.Collapsed;
            SpellingContextSeparator.Visibility = Visibility.Collapsed;
            return;
        }

        int caretOffset = Editor.CaretOffset;
        var hit = _misspelledSpans.FirstOrDefault(s => caretOffset >= s.Start && caretOffset <= s.Start + s.Length);
        if (hit.Length == 0)
        {
            SpellingContextMenu.Visibility = Visibility.Collapsed;
            SpellingContextSeparator.Visibility = Visibility.Collapsed;
            return;
        }

        string word = Editor.Text.Substring(hit.Start, hit.Length);
        var suggestions = _spellChecker.Suggest(word).Take(5).ToList();

        SpellingContextMenu.Header = $"Spelling: \"{word}\"";

        if (suggestions.Count == 0)
        {
            SpellingContextMenu.Items.Add(new MenuItem { Header = "(no suggestions)", IsEnabled = false });
        }
        else
        {
            foreach (string suggestion in suggestions)
            {
                var item = new MenuItem { Header = suggestion, FontWeight = FontWeights.Bold };
                item.Click += (_, _) => ReplaceMisspelledWord(hit, suggestion);
                SpellingContextMenu.Items.Add(item);
            }
        }

        SpellingContextMenu.Items.Add(new Separator());
        var addToDictionary = new MenuItem { Header = "Add to Dictionary" };
        addToDictionary.Click += (_, _) =>
        {
            _spellChecker.AddWord(word);
            RescanSpelling(Editor.Text);
        };
        SpellingContextMenu.Items.Add(addToDictionary);

        SpellingContextMenu.Visibility = Visibility.Visible;
        SpellingContextSeparator.Visibility = Visibility.Visible;
    }

    private void ReplaceMisspelledWord(MisspelledSpan span, string replacement)
    {
        Editor.Document.Replace(span.Start, span.Length, replacement);
        Editor.Focus();
    }
}
