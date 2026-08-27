namespace OrderManager.Web.Models;

public class Customer
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Phone { get; set; }
}
