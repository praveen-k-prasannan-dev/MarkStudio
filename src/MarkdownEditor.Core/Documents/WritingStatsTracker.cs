namespace MarkdownEditor.Core.Documents;

/// <summary>
/// Tracks "words written" per day from periodic word-count samples of the active document.
/// A day's total is the sum of positive increases between consecutive same-day samples, so
/// deleting text doesn't count negatively and typing new text always does - this approximates
/// writing effort rather than net document length, since those diverge once editing/revision
/// starts.
/// </summary>
public sealed class WritingStatsTracker
{
    private readonly Dictionary<DateOnly, int> _wordsPerDay;
    private DateOnly? _lastSampleDate;
    private int _lastSampleWordCount;

    public WritingStatsTracker() : this(new Dictionary<DateOnly, int>()) { }

    public WritingStatsTracker(IDictionary<DateOnly, int> initialWordsPerDay) =>
        _wordsPerDay = new Dictionary<DateOnly, int>(initialWordsPerDay);

    public IReadOnlyDictionary<DateOnly, int> WordsPerDay => _wordsPerDay;

    public void RecordSample(DateOnly date, int totalWordCount)
    {
        if (_lastSampleDate != date)
        {
            _lastSampleDate = date;
            _lastSampleWordCount = totalWordCount;
            _wordsPerDay.TryAdd(date, 0);
            return;
        }

        int delta = totalWordCount - _lastSampleWordCount;
        if (delta > 0)
            _wordsPerDay[date] = _wordsPerDay.GetValueOrDefault(date) + delta;
        _lastSampleWordCount = totalWordCount;
    }

    public int WordsOn(DateOnly date) => _wordsPerDay.GetValueOrDefault(date);

    public int WordsInLastDays(DateOnly today, int dayCount)
    {
        int total = 0;
        for (int i = 0; i < dayCount; i++)
            total += WordsOn(today.AddDays(-i));
        return total;
    }

    /// <summary>Consecutive days with words written, ending today (if today already has words) or yesterday (grace period before today's writing starts).</summary>
    public int CurrentStreakDays(DateOnly today)
    {
        DateOnly day = WordsOn(today) > 0 ? today : today.AddDays(-1);
        int streak = 0;
        while (WordsOn(day) > 0)
        {
            streak++;
            day = day.AddDays(-1);
        }
        return streak;
    }
}
