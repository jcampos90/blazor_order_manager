namespace OrderManager.Web.Services;

public enum UrgencyBucket
{
    Overdue,
    Now,
    Today,
    Tomorrow,
    Upcoming
}

public static class UrgencyBuckets
{
    public static UrgencyBucket Classify(DateTime startBy, DateTime now)
    {
        if (PrepSchedule.IsOverdue(startBy, now))
            return UrgencyBucket.Overdue;

        if (startBy <= now.AddMinutes(60))
            return UrgencyBucket.Now;

        if (startBy.Date == now.Date)
            return UrgencyBucket.Today;

        if (startBy.Date == now.Date.AddDays(1))
            return UrgencyBucket.Tomorrow;

        return UrgencyBucket.Upcoming;
    }
}