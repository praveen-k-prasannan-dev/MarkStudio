using WeCantSpell.Hunspell;

namespace MarkdownEditor.Core.Spelling;

/// <summary>Thin wrapper around a Hunspell word list, so the rest of the app depends on this
/// interface rather than the WeCantSpell.Hunspell package directly.</summary>
public sealed class SpellChecker
{
    private readonly WordList _wordList;

    private SpellChecker(WordList wordList) => _wordList = wordList;

    public static SpellChecker LoadFromFiles(string dictionaryPath, string affixPath) =>
        new(WordList.CreateFromFiles(dictionaryPath, affixPath));

    /// <summary>For tests: a tiny in-memory dictionary instead of loading real files from disk.</summary>
    public static SpellChecker CreateFromWords(IEnumerable<string> words) =>
        new(WordList.CreateFromWords(words));

    public bool IsCorrect(string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        return _wordList.Check(word);
    }

    public IReadOnlyList<string> Suggest(string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        return _wordList.Suggest(word).ToList();
    }

    /// <summary>Adds a word to the in-memory dictionary for the rest of this session (not persisted).</summary>
    public void AddWord(string word)
    {
        ArgumentNullException.ThrowIfNull(word);
        _wordList.Add(word);
    }
}
