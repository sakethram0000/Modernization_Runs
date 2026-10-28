using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace ChecklistsModern.Models;

public class ApplicationUser : IdentityUser
{
    // No default value: a user created without a full name must fail the NOT NULL column, as in EF6.
    [Required]
    [StringLength(255)]
    public string FullName { get; set; } = null!;
}
