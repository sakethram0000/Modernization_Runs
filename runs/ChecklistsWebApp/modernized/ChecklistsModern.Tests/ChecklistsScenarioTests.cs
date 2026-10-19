using System.Net;
using System.Text.RegularExpressions;
using ChecklistsModern.Data;
using ChecklistsModern.Models;
using ChecklistsModern.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChecklistsModern.Tests;

[CollectionDefinition("checklists", DisableParallelization = true)]
public class ChecklistsCollection : ICollectionFixture<ChecklistsAppFactory>
{
}

public class ChecklistsAppFactory : WebApplicationFactory<Program>
{
    public string DbPath { get; } = Path.Combine(Path.GetTempPath(), $"checklists-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Checklists"] = $"Data Source={DbPath}",
                ["Preview:SignInAs"] = "",
                ["Preview:Password"] = "Preview123!"
            });
        });
    }
}

[Collection("checklists")]
public class ChecklistsScenarioTests
{
    private const string Password = "Preview123!";
    private readonly ChecklistsAppFactory _factory;

    public ChecklistsScenarioTests(ChecklistsAppFactory factory)
    {
        _factory = factory;
    }

    [Fact(DisplayName = "Signed-in user sees only their own checklists")]
    public async Task SignedInUserSeesOnlyTheirOwnChecklists()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var html = await ReadAsync(client, "/Checklists");

        Assert.Contains("My Checklists", html);
        Assert.Contains("Packing list", html);
        Assert.Contains("Create new list", html);
        Assert.DoesNotContain("Other list", html);
    }

    [Fact(DisplayName = "A soft-deleted checklist is hidden from the list")]
    public async Task SoftDeletedChecklistIsHiddenFromTheList()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var html = await ReadAsync(client, "/Checklists");

        Assert.Contains("Packing list", html);
        Assert.DoesNotContain("Old list", html);
    }

    [Fact(DisplayName = "A user with no checklists sees the empty message")]
    public async Task UserWithNoChecklistsSeesTheEmptyMessage()
    {
        await ResetAsync();
        var client = await SignedInClient("empty@checklists.local");
        var html = await ReadAsync(client, "/Checklists");

        Assert.Contains("You don't have any checklists.", html);
        Assert.Contains("Create a checklist", html);
    }

    [Fact(DisplayName = "New checklist form shows an empty name")]
    public async Task NewChecklistFormShowsAnEmptyName()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var html = await ReadAsync(client, "/Checklists/New");

        Assert.Contains("New checklist", html);
        Assert.Contains("Name", html);
        Assert.Contains("Save", html);
        Assert.Contains("Cancel", html);
        var input = Regex.Match(html, "<input[^>]*name=\"Name\"[^>]*>", RegexOptions.IgnoreCase);
        Assert.True(input.Success);
        Assert.DoesNotMatch("value=\"[^\"]+\"", input.Value);
    }

    [Fact(DisplayName = "Save a new checklist and the next request still shows it")]
    public async Task SaveANewChecklistAndTheNextRequestStillShowsIt()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var response = await PostSaveAsync(client, "0", "Airport");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var html = await ReadAsync(client, "/Checklists");
        Assert.Contains("Airport", html);
    }

    [Fact(DisplayName = "Save with an empty name shows the required message")]
    public async Task SaveWithAnEmptyNameShowsTheRequiredMessage()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var response = await PostSaveAsync(client, "0", "");
        var html = Decode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("The Name field is required.", html);
    }

    [Fact(DisplayName = "Save with a name longer than 255 characters shows the length message")]
    public async Task SaveWithANameLongerThan255CharactersShowsTheLengthMessage()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var response = await PostSaveAsync(client, "0", new string('a', 256));
        var html = Decode(await response.Content.ReadAsStringAsync());

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("The field Name must be a string with a maximum length of 255.", html);
    }

    [Fact(DisplayName = "Save without the verification token is rejected")]
    public async Task SaveWithoutTheVerificationTokenIsRejected()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var response = await client.PostAsync("/Checklists/Save", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = "0",
            ["Name"] = "Airport"
        }));
        var html = await ReadAsync(client, "/Checklists");

        Assert.NotEqual(HttpStatusCode.Redirect, response.StatusCode);
        Assert.DoesNotContain("Airport", html);
    }

    [Fact(DisplayName = "The author opens the rename form")]
    public async Task TheAuthorOpensTheRenameForm()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var id = await IdOfAsync("Packing list");
        var html = await ReadAsync(client, $"/Checklists/Edit/{id}");

        Assert.Contains("Modify checklist name", html);
        Assert.Contains("Packing list", html);
    }

    [Fact(DisplayName = "Edit without an id is not found")]
    public async Task EditWithoutAnIdIsNotFound()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var response = await client.GetAsync("/Checklists/Edit");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "Edit of an unknown id is not found")]
    public async Task EditOfAnUnknownIdIsNotFound()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var response = await client.GetAsync("/Checklists/Edit/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "A different signed-in user cannot open the rename form")]
    public async Task ADifferentSignedInUserCannotOpenTheRenameForm()
    {
        await ResetAsync();
        var other = await SignedInClient("other@checklists.local");
        var id = await IdOfAsync("Packing list");
        var response = await other.GetAsync($"/Checklists/Edit/{id}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact(DisplayName = "The author saves a new name")]
    public async Task TheAuthorSavesANewName()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var id = await IdOfAsync("Packing list");
        var response = await PostSaveAsync(client, id.ToString(), "Travel list");
        var html = await ReadAsync(client, "/Checklists");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Travel list", html);
        Assert.DoesNotContain("Packing list", html);
    }

    [Fact(DisplayName = "Save of an unknown id is not found and does not create a checklist")]
    public async Task SaveOfAnUnknownIdIsNotFoundAndDoesNotCreateAChecklist()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var response = await PostSaveAsync(client, "999999", "Ghost");
        var html = await ReadAsync(client, "/Checklists");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("Ghost", html);
    }

    [Fact(DisplayName = "Save of a soft-deleted checklist changes the name and leaves it deleted")]
    public async Task SaveOfASoftDeletedChecklistChangesTheNameAndLeavesItDeleted()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var id = await IdOfAsync("Old list");
        await PostSaveAsync(client, id.ToString(), "Older list");

        var edit = Decode(await (await client.GetAsync($"/Checklists/Edit/{id}")).Content.ReadAsStringAsync());
        var list = await ReadAsync(client, "/Checklists");
        var deleted = await client.DeleteAsync($"/api/checklists/{id}");

        Assert.Contains("Older list", edit);
        Assert.DoesNotContain("Older list", list);
        Assert.Equal(HttpStatusCode.NotFound, deleted.StatusCode);
    }

    [Fact(DisplayName = "A different signed-in user can change the name through Save")]
    public async Task ADifferentSignedInUserCanChangeTheNameThroughSave()
    {
        await ResetAsync();
        var other = await SignedInClient("other@checklists.local");
        var alex = await SignedInClient("author@checklists.local");
        var id = await IdOfAsync("Packing list");
        var response = await PostSaveAsync(other, id.ToString(), "Changed by other");
        var html = await ReadAsync(alex, "/Checklists");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("Changed by other", html);
    }

    [Fact(DisplayName = "The author deletes a checklist and it leaves the list")]
    public async Task TheAuthorDeletesAChecklistAndItLeavesTheList()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var id = await IdOfAsync("Packing list");
        var response = await client.DeleteAsync($"/api/checklists/{id}");
        var body = await response.Content.ReadAsStringAsync();
        var html = await ReadAsync(client, "/Checklists");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(string.Empty, body);
        Assert.DoesNotContain("Packing list", html);
    }

    [Fact(DisplayName = "Delete of a missing checklist is not found")]
    public async Task DeleteOfAMissingChecklistIsNotFound()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var response = await client.DeleteAsync("/api/checklists/999999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(string.Empty, await response.Content.ReadAsStringAsync());
    }

    [Fact(DisplayName = "Delete of an already deleted checklist is not found")]
    public async Task DeleteOfAnAlreadyDeletedChecklistIsNotFound()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var id = await IdOfAsync("Old list");
        var response = await client.DeleteAsync($"/api/checklists/{id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact(DisplayName = "A different signed-in user cannot delete the checklist")]
    public async Task ADifferentSignedInUserCannotDeleteTheChecklist()
    {
        await ResetAsync();
        var other = await SignedInClient("other@checklists.local");
        var alex = await SignedInClient("author@checklists.local");
        var id = await IdOfAsync("Packing list");
        var response = await other.DeleteAsync($"/api/checklists/{id}");
        var html = await ReadAsync(alex, "/Checklists");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Packing list", html);
    }

    [Fact(DisplayName = "An anonymous delete is rejected")]
    public async Task AnAnonymousDeleteIsRejected()
    {
        await ResetAsync();
        var anonymous = Client();
        var alex = await SignedInClient("author@checklists.local");
        var id = await IdOfAsync("Packing list");
        var response = await anonymous.DeleteAsync($"/api/checklists/{id}");
        var html = await ReadAsync(alex, "/Checklists");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Contains("Packing list", html);
    }

    [Fact(DisplayName = "An anonymous visit to the checklist list is sent to login")]
    public async Task AnAnonymousVisitToTheChecklistListIsSentToLogin()
    {
        await ResetAsync();
        var response = await Client().GetAsync("/Checklists");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString() ?? string.Empty);
    }

    [Fact(DisplayName = "The login page shows the email and password fields")]
    public async Task TheLoginPageShowsTheEmailAndPasswordFields()
    {
        await ResetAsync();
        var html = await ReadAsync(Client(), "/Account/Login");

        Assert.Contains("Log in", html);
        Assert.Contains("Email", html);
        Assert.Contains("Password", html);
        Assert.Contains("Remember me?", html);
        Assert.Contains("Register", html);
    }

    [Fact(DisplayName = "A signed-in checklist page shows the author header")]
    public async Task ASignedInChecklistPageShowsTheAuthorHeader()
    {
        await ResetAsync();
        var client = await SignedInClient("author@checklists.local");
        var html = await ReadAsync(client, "/Checklists");

        Assert.Contains("Hello Alex Author!", html);
        Assert.Contains("Change Password", html);
        Assert.Contains("Log out", html);
        Assert.Contains("Checklists", html);
    }

    private async Task ResetAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Checklists.ExecuteDeleteAsync();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var seeder = scope.ServiceProvider.GetRequiredService<ChecklistSeeder>();
        var alex = await users.FindByEmailAsync("author@checklists.local");
        var other = await users.FindByEmailAsync("other@checklists.local");
        await seeder.SeedChecklistsAsync(alex!.Id, other!.Id);
    }

    private async Task<int> IdOfAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        return await db.Checklists.Where(checklist => checklist.Name == name).Select(checklist => checklist.Id).SingleAsync();
    }

    private HttpClient Client()
    {
        return _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });
    }

    private async Task<HttpClient> SignedInClient(string email)
    {
        var client = Client();
        var page = await client.GetAsync("/Account/Login");
        var token = Token(await page.Content.ReadAsStringAsync());
        var response = await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = Password,
            ["__RequestVerificationToken"] = token
        }));

        if (response.StatusCode != HttpStatusCode.Redirect)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Login for {email} returned {(int)response.StatusCode}. {body}");
        }

        return client;
    }

    private async Task<HttpResponseMessage> PostSaveAsync(HttpClient client, string id, string name)
    {
        var page = await client.GetAsync("/Checklists/New");
        var token = Token(await page.Content.ReadAsStringAsync());
        return await client.PostAsync("/Checklists/Save", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Id"] = id,
            ["Name"] = name,
            ["__RequestVerificationToken"] = token
        }));
    }

    private static async Task<string> ReadAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        var body = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"{url} returned {(int)response.StatusCode}. {body}");
        }

        return Decode(body);
    }

    private static string Token(string html)
    {
        var input = Regex.Match(html, "<input[^>]*name=\"__RequestVerificationToken\"[^>]*>", RegexOptions.IgnoreCase);
        if (!input.Success)
        {
            throw new InvalidOperationException("Antiforgery token was not on the page.");
        }

        var value = Regex.Match(input.Value, "value=\"([^\"]+)\"", RegexOptions.IgnoreCase);
        return Decode(value.Groups[1].Value);
    }

    private static string Decode(string html)
    {
        return WebUtility.HtmlDecode(html);
    }
}
