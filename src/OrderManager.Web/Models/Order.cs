namespace OrderManager.Web.Models;

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public DateTime DeliveryAt { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<OrderLine> Lines { get; set; } = [];

    public decimal Total => Lines.Sum(l => l.Quantity * l.UnitPrice);
}
