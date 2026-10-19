using ChecklistsModern.Data;
using ChecklistsModern.Models;
using Microsoft.EntityFrameworkCore;

namespace ChecklistsModern.Services;

public class ChecklistService : IChecklistService
{
    private readonly ApplicationDbContext _db;
    private readonly ILogger<ChecklistService> _logger;

    public ChecklistService(ApplicationDbContext db, ILogger<ChecklistService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Checklist>> ListAsync(string authorId)
    {
        return await _db.Checklists
            .Where(checklist => checklist.AuthorId == authorId && !checklist.IsDeleted)
            .ToListAsync();
    }

    public async Task<Checklist?> GetAsync(int id)
    {
        return await _db.Checklists.SingleOrDefaultAsync(checklist => checklist.Id == id);
    }

    public async Task AddAsync(string name, string authorId)
    {
        var checklist = new Checklist
        {
            Name = name,
            AuthorId = authorId
        };

        _db.Checklists.Add(checklist);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created checklist {ChecklistId} for {AuthorId}", checklist.Id, authorId);
    }

    public async Task<bool> RenameAsync(int id, string name)
    {
        var checklist = await GetAsync(id);
        if (checklist is null)
        {
            return false;
        }

        checklist.Name = name;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Renamed checklist {ChecklistId}", id);
        return true;
    }

    public async Task<DeleteResult> DeleteAsync(int id, string authorId)
    {
        var checklist = await GetAsync(id);
        if (checklist is null || checklist.IsDeleted)
        {
            return DeleteResult.NotFound;
        }

        if (checklist.AuthorId != authorId)
        {
            return DeleteResult.NotAuthor;
        }

        checklist.Delete();
        await _db.SaveChangesAsync();
        _logger.LogInformation("Soft-deleted checklist {ChecklistId}", id);
        return DeleteResult.Deleted;
    }
}
