using System.Windows;
using MarkdownEditor.Core.Documents;

namespace MarkdownEditor.App;

/// <summary>Tracks and displays "words written" per day, streaks, and a trailing 7-day total.</summary>
public partial class MainWindow
{
    private readonly WritingStatsTracker _writingStats = Services.WritingStatsService.Load();

    private void RecordWritingStatsSample(string text)
    {
        int words = DocumentStatistics.Compute(text).Words;
        _writingStats.RecordSample(DateOnly.FromDateTime(DateTime.Now), words);
    }

    private void SaveWritingStats() => Services.WritingStatsService.Save(_writingStats);

    private void StatsDropdownButton_Checked(object sender, RoutedEventArgs e)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        StatsTodayLabel.Text = $"Today: {_writingStats.WordsOn(today)} words";
        StatsWeekLabel.Text = $"Last 7 days: {_writingStats.WordsInLastDays(today, 7)} words";
        int streak = _writingStats.CurrentStreakDays(today);
        StatsStreakLabel.Text = streak switch
        {
            0 => "No current streak",
            1 => "Streak: 1 day",
            _ => $"Streak: {streak} days",
        };
    }
}
