using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ChecklistsModern.Models;

public class Checklist
{
    public Checklist()
    {
        DateAdded = DateTime.Now;
        Tasks = new List<ChecklistTask>();
    }

    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    public DateTime DateAdded { get; set; }

    public bool IsDeleted { get; private set; }

    [Required]
    public string AuthorId { get; set; } = null!;

    [ValidateNever]
    public ApplicationUser? Author { get; set; }

    [ValidateNever]
    public ICollection<ChecklistTask> Tasks { get; set; }

    public void Delete()
    {
        IsDeleted = true;
    }
}
