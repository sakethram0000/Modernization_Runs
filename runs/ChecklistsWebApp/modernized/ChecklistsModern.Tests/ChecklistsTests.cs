using System.Net;
using ChecklistsModern.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChecklistsModern.Tests;

public class ChecklistsTests
{
    private static Dictionary<string, string?> ChecklistForm(int id, string name) =>
        new() { ["Id"] = id.ToString(), ["Name"] = name };

    // US-CL-01 See my checklists

    [Fact(DisplayName = "US-CL-01 | Signed-in user sees only their own checklists")]
    public async Task Sees_only_own_checklists()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/");

        Assert.Contains("My Checklists", html);
        Assert.Contains("href=\"/Checklists/New\" class=\"btn btn-primary\"", html);
        Assert.Contains("Create new list", html);
        Assert.Contains(">Packing list</a>", html);
        Assert.DoesNotContain("Other list", html);
    }

    [Fact(DisplayName = "US-CL-01 | A soft-deleted checklist is hidden from the list")]
    public async Task Soft_deleted_checklist_hidden()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.DoesNotContain("Old list", await browser.GetHtmlAsync("/"));
    }

    [Fact(DisplayName = "US-CL-01 | Each checklist has edit and delete controls")]
    public async Task Checklist_has_edit_and_delete()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/");

        Assert.Contains("<a href=\"/checklists/1/tasks\">Packing list</a>", html);
        Assert.Contains("<a href=\"/Checklists/Edit/1\" title=\"Edit\">", html);
        Assert.Contains("<a href=\"#\" data-checklist-id=\"1\" class=\"js-delete\" title=\"Delete\">", html);
    }

    [Fact(DisplayName = "US-CL-01 | A user with no checklists sees the empty message")]
    public async Task Empty_user_sees_empty_message()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Empty);
        var html = await browser.GetHtmlAsync("/");

        Assert.Contains("You don't have any checklists.", html);
        Assert.Contains("<a href=\"/Checklists/New\">Create a checklist</a>", html);
    }

    [Fact(DisplayName = "US-CL-01 | An anonymous visit to the checklist list is sent to login")]
    public async Task Anonymous_list_goes_to_login()
    {
        using var app = new TestApp();

        (await app.NewBrowser().GetAsync("/")).AssertRedirect("/Account/Login?ReturnUrl=%2F");
    }

    // US-CL-02 Create a checklist

    [Fact(DisplayName = "US-CL-02 | New checklist form shows an empty name")]
    public async Task New_checklist_form()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/Checklists/New");

        Assert.Contains("<h2>New checklist</h2>", html);
        Assert.Contains("<label for=\"Name\">Name</label>", html);
        Assert.Contains("id=\"Name\" name=\"Name\" type=\"text\" value=\"\"", html);
        Assert.Contains("name=\"Id\" type=\"hidden\" value=\"0\"", html);
        Assert.Contains("<button type=\"submit\" class=\"btn btn-primary\">Save</button>", html);
        Assert.Contains(">Cancel</a>", html);
    }

    [Fact(DisplayName = "US-CL-02 | Save a new checklist and the next request still shows it")]
    public async Task Save_new_checklist_persists()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var author = await app.UserAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Checklists/Save", ChecklistForm(0, "Groceries"), tokenPage: "/");

        response.AssertRedirect("/");
        Assert.Contains("Groceries", await (await app.SignedInAsync(TestApp.Author)).GetHtmlAsync("/"));
        var stored = await app.DbAsync(db => db.Checklists.AsNoTracking().SingleAsync(c => c.Name == "Groceries"));
        Assert.Equal(author.Id, stored.AuthorId);
        Assert.False(stored.IsDeleted);
        Assert.NotEqual(default, stored.DateAdded);
    }

    [Fact(DisplayName = "US-CL-02 | Save with an empty name shows the required message")]
    public async Task Save_empty_name()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Checklists/Save", ChecklistForm(0, ""), tokenPage: "/");

        Assert.Contains("The Name field is required.", await response.HtmlAsync());
        Assert.Equal(3, await app.DbAsync(db => db.Checklists.CountAsync()));
    }

    [Fact(DisplayName = "US-CL-02 | Save with a name longer than 255 characters shows the length message")]
    public async Task Save_long_name()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Checklists/Save", ChecklistForm(0, new string('a', 256)), tokenPage: "/");

        Assert.Contains("The field Name must be a string with a maximum length of 255.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-CL-02 | Save without the verification token is rejected")]
    public async Task Save_without_token()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormWithoutTokenAsync("/Checklists/Save", ChecklistForm(0, "No token"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(await app.DbAsync(db => db.Checklists.AnyAsync(c => c.Name == "No token")));
    }

    // US-CL-03 Rename a checklist

    [Fact(DisplayName = "US-CL-03 | The author opens the rename form")]
    public async Task Author_opens_rename_form()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/Checklists/Edit/1");

        Assert.Contains("<h2>Modify checklist name</h2>", html);
        Assert.Contains("name=\"Name\" type=\"text\" value=\"Packing list\"", html);
    }

    [Fact(DisplayName = "US-CL-03 | Edit without an id is not found")]
    public async Task Edit_without_id()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/Checklists/Edit")).StatusCode);
    }

    [Fact(DisplayName = "US-CL-03 | Edit of an unknown id is not found")]
    public async Task Edit_unknown_id()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/Checklists/Edit/9999")).StatusCode);
    }

    [Fact(DisplayName = "US-CL-03 | A different signed-in user cannot open the rename form")]
    public async Task Other_user_cannot_open_rename()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        (await browser.GetAsync("/Checklists/Edit/1")).AssertRedirectToLogin();
    }

    [Fact(DisplayName = "US-CL-03 | The author saves a new name")]
    public async Task Author_saves_new_name()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Checklists/Save", ChecklistForm(TestApp.PackingList, "Holiday packing"), tokenPage: "/");

        response.AssertRedirect("/");
        Assert.Contains("Holiday packing", await browser.GetHtmlAsync("/"));
    }

    [Fact(DisplayName = "US-CL-03 | Save of an unknown id is not found and does not create a checklist")]
    public async Task Save_unknown_id()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Checklists/Save", ChecklistForm(9999, "Ghost"), tokenPage: "/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(await app.DbAsync(db => db.Checklists.AnyAsync(c => c.Name == "Ghost")));
    }

    [Fact(DisplayName = "US-CL-03 | Save of a soft-deleted checklist changes the name and leaves it deleted")]
    public async Task Save_soft_deleted_checklist()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        await browser.PostFormAsync("/Checklists/Save", ChecklistForm(TestApp.OldList, "Renamed old"), tokenPage: "/");

        var stored = await app.ChecklistAsync(TestApp.OldList);
        Assert.Equal("Renamed old", stored!.Name);
        Assert.True(stored.IsDeleted);
        Assert.DoesNotContain("Renamed old", await browser.GetHtmlAsync("/"));
    }

    [Fact(DisplayName = "US-CL-03 | A different signed-in user can change the name through Save")]
    public async Task Other_user_can_rename_through_save()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        await browser.PostFormAsync("/Checklists/Save", ChecklistForm(TestApp.PackingList, "Changed by other"), tokenPage: "/");

        Assert.Equal("Changed by other", (await app.ChecklistAsync(TestApp.PackingList))!.Name);
    }

    // US-CL-04 Delete a checklist

    [Fact(DisplayName = "US-CL-04 | The author deletes a checklist and it leaves the list")]
    public async Task Author_deletes_checklist()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.AjaxAsync(HttpMethod.Delete, "/api/checklists/1");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
        Assert.DoesNotContain("Packing list", await browser.GetHtmlAsync("/"));
        Assert.True((await app.ChecklistAsync(TestApp.PackingList))!.IsDeleted);
    }

    [Fact(DisplayName = "US-CL-04 | Delete of a missing checklist is not found")]
    public async Task Delete_missing_checklist()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.AjaxAsync(HttpMethod.Delete, "/api/checklists/9999")).StatusCode);
    }

    [Fact(DisplayName = "US-CL-04 | Delete of an already deleted checklist is not found")]
    public async Task Delete_already_deleted_checklist()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.AjaxAsync(HttpMethod.Delete, $"/api/checklists/{TestApp.OldList}")).StatusCode);
    }

    [Fact(DisplayName = "US-CL-04 | A different signed-in user cannot delete the checklist")]
    public async Task Other_user_cannot_delete()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.AjaxAsync(HttpMethod.Delete, "/api/checklists/1")).StatusCode);
        Assert.False((await app.ChecklistAsync(TestApp.PackingList))!.IsDeleted);
    }

    [Fact(DisplayName = "US-CL-04 | An anonymous delete is rejected")]
    public async Task Anonymous_delete_rejected()
    {
        using var app = new TestApp();

        Assert.Equal(HttpStatusCode.Unauthorized, (await app.NewBrowser().AjaxAsync(HttpMethod.Delete, "/api/checklists/1")).StatusCode);
        Assert.False((await app.ChecklistAsync(TestApp.PackingList))!.IsDeleted);
    }

    // US-CL-05 Delete confirmation on the list

    [Fact(DisplayName = "US-CL-05 | The list page carries the legacy delete script")]
    public async Task List_page_has_delete_script()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/");

        Assert.Contains("Are you sure that you want to delete the checklist?", html);
        Assert.Contains("url: \"api/checklists/\" + button.attr(\"data-checklist-id\")", html);
        Assert.Contains("<script src=\"/Scripts/bootbox.js\"></script>", html);
    }
}
