namespace OrderManager.Web.Models;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public int PrepHours { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;
}
