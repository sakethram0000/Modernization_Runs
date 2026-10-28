using ChecklistsModern.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChecklistsModern.Data;

// Fake preview rows listed in plan.md. Not legacy data.
public static class PreviewSeeder
{
    public const string PreviewPassword = "Preview123!";

    public static async Task SeedAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (await context.Users.AnyAsync())
            return;

        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var author = await CreateUserAsync(userManager, "author@checklists.local", "Alex Author");
        var other = await CreateUserAsync(userManager, "other@checklists.local", "Other User");
        await CreateUserAsync(userManager, "empty@checklists.local", "Empty User");
        var twoFactor = await CreateUserAsync(userManager, "twofactor@checklists.local", "Tina Twofactor");
        twoFactor.EmailConfirmed = true;
        twoFactor.TwoFactorEnabled = true;
        await userManager.UpdateAsync(twoFactor);

        var packing = new Checklist { Name = "Packing list", AuthorId = author.Id };
        context.Checklists.Add(packing);
        await context.SaveChangesAsync();

        var oldList = new Checklist { Name = "Old list", AuthorId = author.Id };
        oldList.Delete();
        context.Checklists.Add(oldList);
        await context.SaveChangesAsync();

        var otherList = new Checklist { Name = "Other list", AuthorId = other.Id };
        context.Checklists.Add(otherList);
        await context.SaveChangesAsync();

        context.Tasks.Add(new ChecklistTask { Title = "Passport", ChecklistId = packing.Id });
        await context.SaveChangesAsync();
        context.Tasks.Add(new ChecklistTask { Title = "Charger", ChecklistId = packing.Id, Checked = true });
        await context.SaveChangesAsync();
        context.Tasks.Add(new ChecklistTask { Title = "Old task", ChecklistId = packing.Id, IsDeleted = true });
        await context.SaveChangesAsync();
        context.Tasks.Add(new ChecklistTask { Title = "Other task", ChecklistId = otherList.Id });
        await context.SaveChangesAsync();
    }

    private static async Task<ApplicationUser> CreateUserAsync(UserManager<ApplicationUser> userManager, string email, string fullName)
    {
        var user = new ApplicationUser { UserName = email, Email = email, FullName = fullName };
        var result = await userManager.CreateAsync(user, PreviewPassword);
        if (!result.Succeeded)
            throw new InvalidOperationException($"Could not seed {email}: {string.Join(" ", result.Errors.Select(e => e.Description))}");
        return user;
    }
}
