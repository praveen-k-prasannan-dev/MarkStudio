using MarkdownEditor.Core.Documents;

namespace MarkdownEditor.Core.Tests;

public class WritingStatsTrackerTests
{
    private static readonly DateOnly Day1 = new(2026, 7, 20);
    private static readonly DateOnly Day2 = new(2026, 7, 21);
    private static readonly DateOnly Day3 = new(2026, 7, 22);

    [Fact]
    public void First_sample_of_a_day_establishes_baseline_with_zero_words()
    {
        var tracker = new WritingStatsTracker();

        tracker.RecordSample(Day1, 50);

        Assert.Equal(0, tracker.WordsOn(Day1));
    }

    [Fact]
    public void Subsequent_same_day_increase_adds_to_the_day_total()
    {
        var tracker = new WritingStatsTracker();

        tracker.RecordSample(Day1, 50);
        tracker.RecordSample(Day1, 80);
        tracker.RecordSample(Day1, 120);

        Assert.Equal(70, tracker.WordsOn(Day1));
    }

    [Fact]
    public void Decrease_does_not_subtract_from_the_day_total()
    {
        var tracker = new WritingStatsTracker();

        tracker.RecordSample(Day1, 100);
        tracker.RecordSample(Day1, 40); // big deletion/undo
        tracker.RecordSample(Day1, 60); // typed 20 new words back

        Assert.Equal(20, tracker.WordsOn(Day1));
    }

    [Fact]
    public void New_day_starts_a_fresh_baseline_without_carrying_over_the_previous_days_delta()
    {
        var tracker = new WritingStatsTracker();

        tracker.RecordSample(Day1, 100);
        tracker.RecordSample(Day1, 150); // +50 on day 1
        tracker.RecordSample(Day2, 150); // same count, new day: baseline reset, no delta
        tracker.RecordSample(Day2, 170); // +20 on day 2

        Assert.Equal(50, tracker.WordsOn(Day1));
        Assert.Equal(20, tracker.WordsOn(Day2));
    }

    [Fact]
    public void WordsInLastDays_sums_the_trailing_window_including_today()
    {
        var tracker = new WritingStatsTracker();
        tracker.RecordSample(Day1, 0);
        tracker.RecordSample(Day1, 30);
        tracker.RecordSample(Day2, 30);
        tracker.RecordSample(Day2, 50);
        tracker.RecordSample(Day3, 50);
        tracker.RecordSample(Day3, 55);

        Assert.Equal(55, tracker.WordsInLastDays(Day3, 3));
    }

    [Fact]
    public void Streak_counts_consecutive_days_with_words_ending_today()
    {
        var tracker = new WritingStatsTracker();
        tracker.RecordSample(Day1, 0);
        tracker.RecordSample(Day1, 10);
        tracker.RecordSample(Day2, 10);
        tracker.RecordSample(Day2, 25);
        tracker.RecordSample(Day3, 25);
        tracker.RecordSample(Day3, 40);

        Assert.Equal(3, tracker.CurrentStreakDays(Day3));
    }

    [Fact]
    public void Streak_has_a_grace_period_for_today_before_any_writing_happens()
    {
        var tracker = new WritingStatsTracker();
        tracker.RecordSample(Day1, 0);
        tracker.RecordSample(Day1, 10);
        tracker.RecordSample(Day2, 10);
        tracker.RecordSample(Day2, 25);

        // Day3 has no samples at all yet (app just opened) - yesterday's streak should still count.
        Assert.Equal(2, tracker.CurrentStreakDays(Day3));
    }

    [Fact]
    public void Streak_breaks_after_a_day_with_no_words_written()
    {
        var tracker = new WritingStatsTracker();
        tracker.RecordSample(Day1, 0);
        tracker.RecordSample(Day1, 10);
        // Day2: opened the app but wrote nothing (sample equals baseline every time).
        tracker.RecordSample(Day2, 10);
        tracker.RecordSample(Day3, 10);
        tracker.RecordSample(Day3, 20);

        Assert.Equal(1, tracker.CurrentStreakDays(Day3));
    }

    [Fact]
    public void Constructor_accepts_previously_persisted_totals()
    {
        var initial = new Dictionary<DateOnly, int> { [Day1] = 300, [Day2] = 150 };

        var tracker = new WritingStatsTracker(initial);

        Assert.Equal(300, tracker.WordsOn(Day1));
        Assert.Equal(150, tracker.WordsOn(Day2));
    }

    [Fact]
    public void Unrecorded_day_returns_zero()
    {
        var tracker = new WritingStatsTracker();

        Assert.Equal(0, tracker.WordsOn(Day1));
    }
}
