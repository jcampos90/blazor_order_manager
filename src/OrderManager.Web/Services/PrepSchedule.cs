namespace OrderManager.Web.Services;

public static class PrepSchedule
{
    public static DateTime StartBy(DateTime deliveryAt, int prepHours) =>
        deliveryAt.AddHours(-prepHours);

    public static bool IsOverdue(DateTime startBy, DateTime now) => startBy < now;
}
