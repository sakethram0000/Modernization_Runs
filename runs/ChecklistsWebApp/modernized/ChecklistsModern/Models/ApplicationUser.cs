using Microsoft.AspNetCore.Identity;

namespace ChecklistsModern.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}
