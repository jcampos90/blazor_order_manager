using OrderManager.Web.Services;

namespace OrderManager.Web.Tests;

public class UrgencyBucketsTests
{
    [Fact]
    public void Classify_StartByBeforeNow_IsOverdue()
    {
        var now = new DateTime(2026, 8, 14, 9, 0, 0);
        var startBy = new DateTime(2026, 8, 14, 8, 59, 59);

        Assert.Equal(UrgencyBucket.Overdue, UrgencyBuckets.Classify(startBy, now));
    }

    [Fact]
    public void Classify_StartByEqualsNow_IsNow()
    {
        var now = new DateTime(2026, 8, 14, 9, 0, 0);

        Assert.Equal(UrgencyBucket.Now, UrgencyBuckets.Classify(now, now));
    }

    [Fact]
    public void Classify_StartByWithinOneHour_IsNow()
    {
        var now = new DateTime(2026, 8, 14, 9, 0, 0);
        var startBy = new DateTime(2026, 8, 14, 9, 30, 0);

        Assert.Equal(UrgencyBucket.Now, UrgencyBuckets.Classify(startBy, now));
    }

    [Fact]
    public void Classify_StartByExactlyOneHourAhead_IsNow()
    {
        var now = new DateTime(2026, 8, 14, 9, 0, 0);
        var startBy = new DateTime(2026, 8, 14, 10, 0, 0);

        Assert.Equal(UrgencyBucket.Now, UrgencyBuckets.Classify(startBy, now));
    }

    [Fact]
    public void Classify_StartByJustPastOneHourSameDay_IsToday()
    {
        var now = new DateTime(2026, 8, 14, 9, 0, 0);
        var startBy = new DateTime(2026, 8, 14, 10, 0, 1);

        Assert.Equal(UrgencyBucket.Today, UrgencyBuckets.Classify(startBy, now));
    }

    [Fact]
    public void Classify_StartByLaterToday_IsToday()
    {
        var now = new DateTime(2026, 8, 14, 9, 0, 0);
        var startBy = new DateTime(2026, 8, 14, 23, 0, 0);

        Assert.Equal(UrgencyBucket.Today, UrgencyBuckets.Classify(startBy, now));
    }

    [Fact]
    public void Classify_StartByTomorrow_IsTomorrow()
    {
        var now = new DateTime(2026, 8, 14, 9, 0, 0);
        var startBy = new DateTime(2026, 8, 15, 8, 0, 0);

        Assert.Equal(UrgencyBucket.Tomorrow, UrgencyBuckets.Classify(startBy, now));
    }

    [Fact]
    public void Classify_StartByDayAfterTomorrow_IsUpcoming()
    {
        var now = new DateTime(2026, 8, 14, 9, 0, 0);
        var startBy = new DateTime(2026, 8, 16, 8, 0, 0);

        Assert.Equal(UrgencyBucket.Upcoming, UrgencyBuckets.Classify(startBy, now));
    }

    [Fact]
    public void Classify_HourWindowTakesPriorityOverTomorrow()
    {
        var now = new DateTime(2026, 8, 14, 23, 30, 0);
        var startBy = new DateTime(2026, 8, 15, 0, 15, 0);

        Assert.Equal(UrgencyBucket.Now, UrgencyBuckets.Classify(startBy, now));
    }
}