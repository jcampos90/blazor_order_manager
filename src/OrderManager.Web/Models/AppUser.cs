using Microsoft.AspNetCore.Identity;

namespace OrderManager.Web.Models;

public class AppUser : IdentityUser
{
    public string? DisplayName { get; set; }

    public bool MustChangePassword { get; set; }
}
