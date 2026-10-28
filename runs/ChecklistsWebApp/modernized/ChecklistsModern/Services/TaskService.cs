using ChecklistsModern.Data;
using ChecklistsModern.Models;
using Microsoft.EntityFrameworkCore;

namespace ChecklistsModern.Services;

public interface ITaskService
{
    void Add(ChecklistTask task);

    Task<ChecklistTask?> GetTaskFromChecklistAsync(int checklistId, int taskId);

    Task<ChecklistTask?> GetTaskWithChecklistAsync(int taskId);

    Task<List<ChecklistTask>> GetTasksFromChecklistAsync(int checklistId);

    Task CompleteAsync();
}

public class TaskService : ITaskService
{
    private readonly ApplicationDbContext _context;

    public TaskService(ApplicationDbContext context)
    {
        _context = context;
    }

    public void Add(ChecklistTask task)
    {
        _context.Tasks.Add(task);
    }

    public Task<ChecklistTask?> GetTaskFromChecklistAsync(int checklistId, int taskId)
    {
        return _context.Tasks
            .Include(t => t.Checklist)
            .SingleOrDefaultAsync(t => t.Id == taskId && t.ChecklistId == checklistId && !t.IsDeleted);
    }

    // No IsDeleted filter: the API reports "Cannot check a deleted item" and deletes a deleted task again, as in the legacy repository.
    public Task<ChecklistTask?> GetTaskWithChecklistAsync(int taskId)
    {
        return _context.Tasks
            .Include(t => t.Checklist)
            .SingleOrDefaultAsync(t => t.Id == taskId);
    }

    public Task<List<ChecklistTask>> GetTasksFromChecklistAsync(int checklistId)
    {
        return _context.Tasks
            .Where(t => t.ChecklistId == checklistId && !t.IsDeleted)
            .ToListAsync();
    }

    public Task CompleteAsync()
    {
        return _context.SaveChangesAsync();
    }
}
