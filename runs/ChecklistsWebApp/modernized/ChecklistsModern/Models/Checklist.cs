using System.ComponentModel.DataAnnotations;

namespace ChecklistsModern.Models;

public class Checklist
{
    public Checklist()
    {
        DateAdded = DateTime.Now;
    }

    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public DateTime DateAdded { get; set; }

    public bool IsDeleted { get; private set; }

    [Required]
    public string AuthorId { get; set; } = string.Empty;

    public void Delete()
    {
        IsDeleted = true;
    }
}
