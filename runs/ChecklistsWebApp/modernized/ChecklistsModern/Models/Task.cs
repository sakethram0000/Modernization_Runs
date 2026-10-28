using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ChecklistsModern.Models;

// Legacy class name is Task; renamed only in C# to avoid clashing with System.Threading.Tasks.Task. Table stays "Tasks".
public class ChecklistTask
{
    public ChecklistTask()
    {
        DateAdded = DateTime.Now;
    }

    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    public bool Checked { get; set; }

    public DateTime DateAdded { get; set; }

    public int ChecklistId { get; set; }

    [ValidateNever]
    public Checklist? Checklist { get; set; }

    public bool IsDeleted { get; set; }

    public void Delete()
    {
        IsDeleted = true;
    }

    public void Check()
    {
        if (!Checked)
            Checked = true;
    }

    public void Uncheck()
    {
        if (Checked)
            Checked = false;
    }
}
