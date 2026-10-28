using ChecklistsModern.Data;
using ChecklistsModern.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ChecklistsModern.Tests.Infrastructure;

// One app and one SQLite file per test, seeded with the preview rows from plan.md.
public sealed class TestApp : WebApplicationFactory<Program>
{
    public const string Author = "author@checklists.local";
    public const string Other = "other@checklists.local";
    public const string Empty = "empty@checklists.local";
    public const string TwoFactor = "twofactor@checklists.local";
    public const string Password = PreviewSeeder.PreviewPassword;

    public const int PackingList = 1;
    public const int OldList = 2;
    public const int OtherList = 3;

    public const int Passport = 1;
    public const int Charger = 2;
    public const int OldTask = 3;
    public const int OtherTask = 4;

    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"checklists-test-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting("ConnectionStrings:DefaultConnection", $"Data Source={_databasePath}");
        builder.UseSetting("Database:Provider", "Sqlite");
        builder.UseSetting("Database:EnsureCreated", "true");
        builder.UseSetting("Database:SeedPreviewData", "true");
    }

    public Browser NewBrowser() =>
        new(CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = true }));

    public async Task<Browser> SignedInAsync(string email, string password = Password)
    {
        var browser = NewBrowser();
        await browser.SignInAsync(email, password);
        return browser;
    }

    public async Task<T> DbAsync<T>(Func<ApplicationDbContext, Task<T>> query)
    {
        await using var scope = Services.CreateAsyncScope();
        return await query(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    public async Task DbAsync(Func<ApplicationDbContext, Task> action)
    {
        await using var scope = Services.CreateAsyncScope();
        await action(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());
    }

    public async Task<T> UsersAsync<T>(Func<UserManager<ApplicationUser>, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        return await action(scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>());
    }

    public Task<ApplicationUser> UserAsync(string email) =>
        UsersAsync(async users => (await users.FindByEmailAsync(email))!);

    public Task<Checklist?> ChecklistAsync(int id) =>
        DbAsync(db => db.Checklists.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id));

    public Task<ChecklistTask?> TaskAsync(int id) =>
        DbAsync(db => db.Tasks.AsNoTracking().SingleOrDefaultAsync(t => t.Id == id));

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        SqliteConnection.ClearAllPools();
        foreach (var file in new[] { _databasePath, _databasePath + "-wal", _databasePath + "-shm" })
        {
            try { File.Delete(file); } catch (IOException) { }
        }
    }
}
