namespace OrderManager.Web.Models;

/// <summary>
/// The single tenant owner of the app: the Clerk user ID (sub) of the baker admitted
/// to the app. A single-row table; first write wins.
/// </summary>
public class AppOwner
{
    public int Id { get; set; }
    public required string ClerkUserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}