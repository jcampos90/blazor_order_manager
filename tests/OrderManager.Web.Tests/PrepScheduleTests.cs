using OrderManager.Web.Services;

namespace OrderManager.Web.Tests;

public class PrepScheduleTests
{
    [Fact]
    public void StartBy_SubtractsPrepTimeFromDelivery()
    {
        var delivery = new DateTime(2026, 8, 15, 8, 0, 0);
        var result = PrepSchedule.StartBy(delivery, 24);
        Assert.Equal(new DateTime(2026, 8, 14, 8, 0, 0), result);
    }

    [Fact]
    public void StartBy_ZeroPrepTime_EqualsDelivery()
    {
        var delivery = new DateTime(2026, 8, 15, 8, 0, 0);
        Assert.Equal(delivery, PrepSchedule.StartBy(delivery, 0));
    }

    [Fact]
    public void StartBy_AcrossMidnight_IsCorrect()
    {
        var delivery = new DateTime(2026, 8, 15, 1, 0, 0);
        var result = PrepSchedule.StartBy(delivery, 30);
        Assert.Equal(new DateTime(2026, 8, 13, 19, 0, 0), result);
    }

    [Fact]
    public void IsOverdue_FalseWhenStartByInFuture()
    {
        var now = new DateTime(2026, 8, 14, 7, 0, 0);
        Assert.False(PrepSchedule.IsOverdue(new DateTime(2026, 8, 14, 8, 0, 0), now));
    }

    [Fact]
    public void IsOverdue_TrueWhenStartByPassed()
    {
        var now = new DateTime(2026, 8, 14, 8, 30, 0);
        Assert.True(PrepSchedule.IsOverdue(new DateTime(2026, 8, 14, 8, 0, 0), now));
    }
}
