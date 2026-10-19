using ChecklistsModern.Data;
using ChecklistsModern.Models;
using ChecklistsModern.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ChecklistsModern.Services;

public class ChecklistSeeder
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly PreviewOptions _preview;

    public ChecklistSeeder(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users,
        IOptions<PreviewOptions> preview)
    {
        _db = db;
        _users = users;
        _preview = preview.Value;
    }

    public async Task SeedAsync()
    {
        if (await _users.Users.AnyAsync())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_preview.Password))
        {
            throw new InvalidOperationException("Preview:Password is required to seed the checklist database.");
        }

        var alex = await CreateUserAsync("author@checklists.local", "Alex Author");
        var other = await CreateUserAsync("other@checklists.local", "Other User");
        await CreateUserAsync("empty@checklists.local", "Empty User");
        await SeedChecklistsAsync(alex.Id, other.Id);
    }

    public async Task SeedChecklistsAsync(string alexId, string otherId)
    {
        var packing = new Checklist
        {
            Name = "Packing list",
            AuthorId = alexId
        };
        var old = new Checklist
        {
            Name = "Old list",
            AuthorId = alexId
        };
        old.Delete();
        var otherList = new Checklist
        {
            Name = "Other list",
            AuthorId = otherId
        };

        _db.Checklists.Add(packing);
        _db.Checklists.Add(old);
        _db.Checklists.Add(otherList);
        await _db.SaveChangesAsync();
    }

    private async Task<ApplicationUser> CreateUserAsync(string email, string fullName)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FullName = fullName
        };

        var result = await _users.CreateAsync(user, _preview.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(error => error.Description));
            throw new InvalidOperationException($"Could not seed {email}. {errors}");
        }

        return user;
    }
}
