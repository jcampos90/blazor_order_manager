namespace OrderManager.Web.Services;

public sealed record PrepScheduleLine(decimal Quantity, string Product, DateTime StartBy);

public static class PrepSchedule
{
    public static DateTime StartBy(DateTime deliveryAt, int prepHours) =>
        deliveryAt.AddHours(-prepHours);

    public static bool IsOverdue(DateTime startBy, DateTime now) => startBy < now;
}
