using ChecklistsModern.Models;

namespace ChecklistsModern.Services;

public interface IChecklistService
{
    Task<IReadOnlyList<Checklist>> ListAsync(string authorId);

    Task<Checklist?> GetAsync(int id);

    Task AddAsync(string name, string authorId);

    Task<bool> RenameAsync(int id, string name);

    Task<DeleteResult> DeleteAsync(int id, string authorId);
}
