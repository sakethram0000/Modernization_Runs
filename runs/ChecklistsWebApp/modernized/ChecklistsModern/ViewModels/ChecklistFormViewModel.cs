using System.ComponentModel.DataAnnotations;
using ChecklistsModern.Models;

namespace ChecklistsModern.ViewModels;

public class ChecklistFormViewModel
{
    public ChecklistFormViewModel()
    {
        Id = 0;
    }

    public ChecklistFormViewModel(Checklist checklist)
    {
        Id = checklist.Id;
        Name = checklist.Name;
    }

    public string Title => Id == 0 ? "New checklist" : "Modify checklist name";

    public int Id { get; set; }

    [Required(ErrorMessage = "The Name field is required.")]
    [StringLength(255, ErrorMessage = "The field Name must be a string with a maximum length of 255.")]
    public string Name { get; set; } = string.Empty;
}
