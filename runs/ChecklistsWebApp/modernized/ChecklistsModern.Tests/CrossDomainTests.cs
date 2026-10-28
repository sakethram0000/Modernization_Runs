using System.Net;
using System.Text.RegularExpressions;
using ChecklistsModern.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChecklistsModern.Tests;

public class CrossDomainTests
{
    private const string Nina = "nina@checklists.local";

    private static async Task<Browser> RegisterNinaAsync(TestApp app)
    {
        var browser = app.NewBrowser();
        var response = await browser.PostFormAsync("/Account/Register", new Dictionary<string, string?>
        {
            ["FullName"] = "Nina New",
            ["Email"] = Nina,
            ["Password"] = "Strong1!",
            ["ConfirmPassword"] = "Strong1!"
        }, tokenPage: "/Account/Register");
        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        return browser;
    }

    private static async Task<int> CreateChecklistAsync(TestApp app, Browser browser, string name)
    {
        var response = await browser.PostFormAsync("/Checklists/Save", new Dictionary<string, string?> { ["Id"] = "0", ["Name"] = name }, tokenPage: "/");
        response.AssertRedirect("/");
        return await app.DbAsync(db => db.Checklists.Where(c => c.Name == name).Select(c => c.Id).SingleAsync());
    }

    private static async Task<int> CreateTaskAsync(TestApp app, Browser browser, int checklistId, string title)
    {
        var response = await browser.PostFormAsync("/Tasks/Save", new Dictionary<string, string?>
        {
            ["Id"] = "0",
            ["ChecklistId"] = checklistId.ToString(),
            ["Title"] = title
        }, tokenPage: "/");
        response.AssertRedirect($"/checklists/{checklistId}/tasks");
        return await app.DbAsync(db => db.Tasks.Where(t => t.Title == title).Select(t => t.Id).SingleAsync());
    }

    // US-XD-01 A new account owns the checklists it creates

    [Fact(DisplayName = "US-XD-01 | Register, create a checklist, and see it on the list")]
    public async Task Register_create_and_list()
    {
        using var app = new TestApp();
        var browser = await RegisterNinaAsync(app);

        var id = await CreateChecklistAsync(app, browser, "Nina's list");

        Assert.Contains("Nina's list", await browser.GetHtmlAsync("/"));
        var nina = await app.UserAsync(Nina);
        Assert.Equal(nina.Id, (await app.ChecklistAsync(id))!.AuthorId);
    }

    [Fact(DisplayName = "US-XD-01 | A checklist created by one account is not on another account's list")]
    public async Task Checklist_not_on_other_account_list()
    {
        using var app = new TestApp();
        await CreateChecklistAsync(app, await RegisterNinaAsync(app), "Nina's list");

        var author = await app.SignedInAsync(TestApp.Author);

        Assert.DoesNotContain("Nina's list", await author.GetHtmlAsync("/"));
    }

    // US-XD-02 Tasks belong to a checklist

    [Fact(DisplayName = "US-XD-02 | Create a checklist, follow its link, and add a task")]
    public async Task Create_follow_and_add_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var id = await CreateChecklistAsync(app, browser, "Weekend");

        var link = Regex.Match(await browser.GetHtmlAsync("/"), "<a href=\"([^\"]+)\">Weekend</a>").Groups[1].Value;
        Assert.Equal($"/checklists/{id}/tasks", link);
        Assert.Matches(@"<h2>\s*Weekend", await browser.GetHtmlAsync(link));

        await CreateTaskAsync(app, browser, id, "Buy bread");

        Assert.Contains("Buy bread", await browser.GetHtmlAsync(link));
    }

    [Fact(DisplayName = "US-XD-02 | Renaming a checklist changes the task page heading")]
    public async Task Rename_changes_task_heading()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Checklists/Save",
            new Dictionary<string, string?> { ["Id"] = TestApp.PackingList.ToString(), ["Name"] = "Trip" }, tokenPage: "/");

        response.AssertRedirect("/");
        Assert.Matches(@"<h2>\s*Trip", await browser.GetHtmlAsync("/checklists/1/tasks"));
    }

    // US-XD-03 A deleted checklist keeps its task page reachable

    [Fact(DisplayName = "US-XD-03 | Delete a checklist and its task page still opens by URL")]
    public async Task Deleted_checklist_task_page_reachable()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NoContent, (await browser.AjaxAsync(HttpMethod.Delete, "/api/checklists/1")).StatusCode);

        Assert.DoesNotContain("Packing list", await browser.GetHtmlAsync("/"));
        Assert.Contains("Passport", await browser.GetHtmlAsync("/checklists/1/tasks"));
    }

    // US-XD-04 Task access follows the checklist author

    [Fact(DisplayName = "US-XD-04 | A user who creates a checklist can work its tasks and another user cannot")]
    public async Task Task_access_follows_author()
    {
        using var app = new TestApp();
        var nina = await RegisterNinaAsync(app);
        var checklistId = await CreateChecklistAsync(app, nina, "Nina's list");
        var taskId = await CreateTaskAsync(app, nina, checklistId, "Nina's task");

        var author = await app.SignedInAsync(TestApp.Author);
        Assert.Equal(HttpStatusCode.Unauthorized, (await author.AjaxAsync(HttpMethod.Put, $"/api/tasks/{taskId}/check")).StatusCode);

        var ninaAgain = await app.SignedInAsync(Nina, "Strong1!");
        Assert.Equal(HttpStatusCode.OK, (await ninaAgain.AjaxAsync(HttpMethod.Put, $"/api/tasks/{taskId}/check")).StatusCode);
        Assert.True((await app.TaskAsync(taskId))!.Checked);
    }

    // US-XD-05 The layout shows the signed-in account

    [Fact(DisplayName = "US-XD-05 | After changing the password the navbar still shows the full name")]
    public async Task Navbar_full_name_after_password_change()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/ChangePassword", new Dictionary<string, string?>
        {
            ["OldPassword"] = TestApp.Password,
            ["NewPassword"] = "Changed1!",
            ["ConfirmPassword"] = "Changed1!"
        }, tokenPage: "/");

        response.AssertRedirect("/");
        Assert.Contains("Hello Alex Author!", await browser.GetHtmlAsync("/"));
    }
}
