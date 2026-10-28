using System.ComponentModel.DataAnnotations;
using ChecklistsModern.Models;

namespace ChecklistsModern.ViewModels;

public class TaskFormViewModel
{
    public TaskFormViewModel()
    {
        Id = 0;
    }

    public TaskFormViewModel(ChecklistTask item)
    {
        Id = item.Id;
        Title = item.Title;
        ChecklistId = item.ChecklistId;
    }

    public string PageTitle
    {
        get { return Id == 0 ? "New Task" : "Modify Task"; }
    }

    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string? Title { get; set; }

    public int ChecklistId { get; set; }
}

public class TaskListingViewModel
{
    public TaskListingViewModel(Checklist checklist, IEnumerable<ChecklistTask> tasks)
    {
        ChecklistId = checklist.Id;
        Name = checklist.Name;
        Tasks = tasks;
    }

    public string Name { get; private set; }

    public int ChecklistId { get; private set; }

    public IEnumerable<ChecklistTask> Tasks { get; private set; }
}
