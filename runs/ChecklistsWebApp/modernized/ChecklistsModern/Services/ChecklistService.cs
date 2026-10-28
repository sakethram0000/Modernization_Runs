using ChecklistsModern.Data;
using ChecklistsModern.Models;
using Microsoft.EntityFrameworkCore;

namespace ChecklistsModern.Services;

public interface IChecklistService
{
    Task<Checklist?> GetChecklistAsync(int id);

    Task<List<Checklist>> GetChecklistsCreatedByUserAsync(string? authorId);

    void Add(Checklist checklist);

    Task CompleteAsync();
}

public class ChecklistService : IChecklistService
{
    private readonly ApplicationDbContext _context;

    public ChecklistService(ApplicationDbContext context)
    {
        _context = context;
    }

    // No IsDeleted filter: Edit, Save, and the task screens still reach a deleted checklist, as in the legacy repository.
    public Task<Checklist?> GetChecklistAsync(int id)
    {
        return _context.Checklists
            .SingleOrDefaultAsync(c => c.Id == id);
    }

    public Task<List<Checklist>> GetChecklistsCreatedByUserAsync(string? authorId)
    {
        return _context.Checklists
            .Where(c => c.AuthorId == authorId && !c.IsDeleted)
            .ToListAsync();
    }

    public void Add(Checklist checklist)
    {
        _context.Checklists.Add(checklist);
    }

    public Task CompleteAsync()
    {
        return _context.SaveChangesAsync();
    }
}
