using System.Net;
using ChecklistsModern.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ChecklistsModern.Tests;

public class TasksTests
{
    private static Dictionary<string, string?> TaskForm(int id, int checklistId, string title) =>
        new() { ["Id"] = id.ToString(), ["ChecklistId"] = checklistId.ToString(), ["Title"] = title };

    private static Task<int> TaskCountAsync(TestApp app) => app.DbAsync(db => db.Tasks.CountAsync());

    // US-TK-01 See the tasks in a checklist

    [Fact(DisplayName = "US-TK-01 | The author sees the checklist name and its tasks")]
    public async Task Author_sees_tasks()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/checklists/1/tasks");

        Assert.Contains("<title>Packing list - Checklists</title>", html);
        Assert.Matches(@"<h2>\s*Packing list", html);
        Assert.Contains("<a href=\"/checklists/1/tasks/new\" class=\"btn btn-primary\">", html);
        Assert.Contains("Add Task", html);
        Assert.Contains("Passport", html);
        Assert.Contains("Charger", html);
    }

    [Fact(DisplayName = "US-TK-01 | A soft-deleted task is hidden")]
    public async Task Soft_deleted_task_hidden()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.DoesNotContain("Old task", await browser.GetHtmlAsync("/checklists/1/tasks"));
    }

    [Fact(DisplayName = "US-TK-01 | A checked task shows a checked box")]
    public async Task Checked_task_shows_checked_box()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/checklists/1/tasks");

        Assert.Contains($"<input type=\"checkbox\" class=\"js-check-toggle\" checked=\"checked\" data-item-id=\"{TestApp.Charger}\" />", html);
        Assert.Contains($"<input type=\"checkbox\" class=\"js-check-toggle\" data-item-id=\"{TestApp.Passport}\" />", html);
    }

    [Fact(DisplayName = "US-TK-01 | Each task has edit and delete controls")]
    public async Task Task_has_edit_and_delete()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/checklists/1/tasks");

        Assert.Contains($"<a href=\"/checklists/1/tasks/edit/{TestApp.Passport}\">", html);
        Assert.Contains($"<a href=\"#\" class=\"js-delete\" data-item-id=\"{TestApp.Passport}\">", html);
    }

    [Fact(DisplayName = "US-TK-01 | A checklist with no tasks shows the empty message")]
    public async Task Empty_checklist_message()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        await browser.PostFormAsync("/Checklists/Save", new Dictionary<string, string?> { ["Id"] = "0", ["Name"] = "Empty list" }, tokenPage: "/");
        var id = await app.DbAsync(db => db.Checklists.Where(c => c.Name == "Empty list").Select(c => c.Id).SingleAsync());

        var html = await browser.GetHtmlAsync($"/checklists/{id}/tasks");

        Assert.Contains("There are no to-do items.", html);
        Assert.Contains($"<a href=\"/checklists/{id}/tasks/new\">Create a new to-do item</a>", html);
    }

    [Fact(DisplayName = "US-TK-01 | An unknown checklist is not found")]
    public async Task Unknown_checklist_not_found()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/checklists/9999/tasks")).StatusCode);
    }

    [Fact(DisplayName = "US-TK-01 | Another user is sent to login")]
    public async Task Other_user_sent_to_login()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        (await browser.GetAsync("/checklists/1/tasks")).AssertRedirectToLogin();
    }

    [Fact(DisplayName = "US-TK-01 | An anonymous visit is sent to login")]
    public async Task Anonymous_sent_to_login()
    {
        using var app = new TestApp();

        (await app.NewBrowser().GetAsync("/checklists/1/tasks")).AssertRedirectToLogin();
    }

    // US-TK-02 Add a task

    [Fact(DisplayName = "US-TK-02 | The new task form shows an empty title for this checklist")]
    public async Task New_task_form()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/checklists/1/tasks/new");

        Assert.Contains("<h2>New Task</h2>", html);
        Assert.Contains("<label for=\"Title\">Title</label>", html);
        Assert.Contains("id=\"Title\" name=\"Title\" type=\"text\" value=\"\"", html);
        Assert.Contains("name=\"Id\" type=\"hidden\" value=\"0\"", html);
        Assert.Contains("name=\"ChecklistId\" type=\"hidden\" value=\"1\"", html);
        Assert.Contains("<button class=\"btn btn-primary\" type=\"submit\">Save</button>", html);
        Assert.Contains("<a class=\"btn btn-link\" href=\"/checklists/1/tasks\">Cancel</a>", html);
    }

    [Fact(DisplayName = "US-TK-02 | New task for an unknown checklist is not found")]
    public async Task New_task_unknown_checklist()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/checklists/9999/tasks/new")).StatusCode);
    }

    [Fact(DisplayName = "US-TK-02 | Another user cannot open the new task form")]
    public async Task Other_user_cannot_open_new_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        (await browser.GetAsync("/checklists/1/tasks/new")).AssertRedirectToLogin();
    }

    [Fact(DisplayName = "US-TK-02 | Save a new task and the task list shows it")]
    public async Task Save_new_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Tasks/Save", TaskForm(0, 1, "Sunscreen"), tokenPage: "/");

        response.AssertRedirect("/checklists/1/tasks");
        var html = await (await app.SignedInAsync(TestApp.Author)).GetHtmlAsync("/checklists/1/tasks");
        Assert.Contains("Sunscreen", html);
        var stored = await app.DbAsync(db => db.Tasks.AsNoTracking().SingleAsync(t => t.Title == "Sunscreen"));
        Assert.False(stored.Checked);
        Assert.Contains($"<input type=\"checkbox\" class=\"js-check-toggle\" data-item-id=\"{stored.Id}\" />", html);
    }

    [Fact(DisplayName = "US-TK-02 | Save with an empty title shows the required message")]
    public async Task Save_empty_title()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var before = await TaskCountAsync(app);

        var response = await browser.PostFormAsync("/Tasks/Save", TaskForm(0, 1, ""), tokenPage: "/");

        Assert.Contains("The Title field is required.", await response.HtmlAsync());
        Assert.Equal(before, await TaskCountAsync(app));
    }

    [Fact(DisplayName = "US-TK-02 | Save with a title longer than 255 characters shows the length message")]
    public async Task Save_long_title()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Tasks/Save", TaskForm(0, 1, new string('a', 256)), tokenPage: "/");

        Assert.Contains("The field Title must be a string with a maximum length of 255.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-TK-02 | Save task without the verification token is rejected")]
    public async Task Save_task_without_token()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var before = await TaskCountAsync(app);

        var response = await browser.PostFormWithoutTokenAsync("/Tasks/Save", TaskForm(0, 1, "No token"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(before, await TaskCountAsync(app));
    }

    // US-TK-03 Rename a task

    [Fact(DisplayName = "US-TK-03 | The author opens the task rename form")]
    public async Task Author_opens_task_rename()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync($"/checklists/1/tasks/edit/{TestApp.Passport}");

        Assert.Contains("<h2>Modify Task</h2>", html);
        Assert.Contains("name=\"Title\" type=\"text\" value=\"Passport\"", html);
    }

    [Fact(DisplayName = "US-TK-03 | Edit of a task in a different checklist id is not found")]
    public async Task Edit_task_in_other_checklist()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync($"/checklists/{TestApp.OtherList}/tasks/edit/{TestApp.Passport}")).StatusCode);
    }

    [Fact(DisplayName = "US-TK-03 | Edit of a soft-deleted task is not found")]
    public async Task Edit_soft_deleted_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var oldTask = await app.TaskAsync(TestApp.OldTask);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync($"/checklists/{oldTask!.ChecklistId}/tasks/edit/{TestApp.OldTask}")).StatusCode);
    }

    [Fact(DisplayName = "US-TK-03 | Another user cannot open the task rename form")]
    public async Task Other_user_cannot_open_task_rename()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        (await browser.GetAsync($"/checklists/1/tasks/edit/{TestApp.Passport}")).AssertRedirectToLogin();
    }

    [Fact(DisplayName = "US-TK-03 | The author saves a new task title")]
    public async Task Author_saves_new_task_title()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Tasks/Save", TaskForm(TestApp.Passport, 1, "Passport and visa"), tokenPage: "/");

        response.AssertRedirect("/checklists/1/tasks");
        Assert.Contains("Passport and visa", await browser.GetHtmlAsync("/checklists/1/tasks"));
    }

    [Fact(DisplayName = "US-TK-03 | Save of an unknown task is not found")]
    public async Task Save_unknown_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Tasks/Save", TaskForm(9999, 1, "Ghost"), tokenPage: "/");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.False(await app.DbAsync(db => db.Tasks.AnyAsync(t => t.Title == "Ghost")));
    }

    // US-TK-04 Task save has no owner check

    [Fact(DisplayName = "US-TK-04 | Another signed-in user can add a task to someone else's checklist")]
    public async Task Other_user_can_add_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        var response = await browser.PostFormAsync("/Tasks/Save", TaskForm(0, 1, "Added by other"), tokenPage: "/");

        response.AssertRedirect("/checklists/1/tasks");
        Assert.True(await app.DbAsync(db => db.Tasks.AnyAsync(t => t.ChecklistId == 1 && t.Title == "Added by other")));
    }

    [Fact(DisplayName = "US-TK-04 | An anonymous visitor can add a task through Save")]
    public async Task Anonymous_can_add_task()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Tasks/Save", TaskForm(0, 1, "Added anonymously"));

        response.AssertRedirect("/checklists/1/tasks");
        Assert.True(await app.DbAsync(db => db.Tasks.AnyAsync(t => t.ChecklistId == 1 && t.Title == "Added anonymously")));
    }

    // US-TK-05 Delete a task

    [Fact(DisplayName = "US-TK-05 | The author deletes a task and it leaves the list")]
    public async Task Author_deletes_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.AjaxAsync(HttpMethod.Delete, $"/api/tasks/{TestApp.Passport}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
        Assert.DoesNotContain("Passport", await browser.GetHtmlAsync("/checklists/1/tasks"));
    }

    [Fact(DisplayName = "US-TK-05 | Delete of a missing task is not found")]
    public async Task Delete_missing_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.AjaxAsync(HttpMethod.Delete, "/api/tasks/9999")).StatusCode);
    }

    [Fact(DisplayName = "US-TK-05 | Delete of an already deleted task succeeds again")]
    public async Task Delete_already_deleted_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NoContent, (await browser.AjaxAsync(HttpMethod.Delete, $"/api/tasks/{TestApp.OldTask}")).StatusCode);
    }

    [Fact(DisplayName = "US-TK-05 | Another user cannot delete the task")]
    public async Task Other_user_cannot_delete_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.AjaxAsync(HttpMethod.Delete, $"/api/tasks/{TestApp.Passport}")).StatusCode);
        Assert.False((await app.TaskAsync(TestApp.Passport))!.IsDeleted);
    }

    [Fact(DisplayName = "US-TK-05 | An anonymous task delete is rejected")]
    public async Task Anonymous_task_delete_rejected()
    {
        using var app = new TestApp();

        Assert.Equal(HttpStatusCode.Unauthorized, (await app.NewBrowser().AjaxAsync(HttpMethod.Delete, $"/api/tasks/{TestApp.Passport}")).StatusCode);
        Assert.False((await app.TaskAsync(TestApp.Passport))!.IsDeleted);
    }

    [Fact(DisplayName = "US-TK-05 | The task list carries the legacy delete and check scripts")]
    public async Task Task_list_has_scripts()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/checklists/1/tasks");

        Assert.Contains("Are you sure that you want to delete the to-do item?", html);
        Assert.Contains("url: \"/api/tasks/\" + checkbox.attr(\"data-item-id\") + \"/check\"", html);
        Assert.Contains("method: \"PUT\"", html);
        Assert.Contains("url: \"/api/tasks/\" + button.attr(\"data-item-id\")", html);
    }

    // US-TK-06 Check a task

    [Fact(DisplayName = "US-TK-06 | Checking a task marks it checked")]
    public async Task Check_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.AjaxAsync(HttpMethod.Put, $"/api/tasks/{TestApp.Passport}/check");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsStringAsync());
        Assert.True((await app.TaskAsync(TestApp.Passport))!.Checked);
    }

    [Fact(DisplayName = "US-TK-06 | Checking a task that is already checked keeps it checked")]
    public async Task Check_checked_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.OK, (await browser.AjaxAsync(HttpMethod.Put, $"/api/tasks/{TestApp.Charger}/check")).StatusCode);
        Assert.True((await app.TaskAsync(TestApp.Charger))!.Checked);
    }

    [Fact(DisplayName = "US-TK-06 | Checking a missing task is not found")]
    public async Task Check_missing_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.AjaxAsync(HttpMethod.Put, "/api/tasks/9999/check")).StatusCode);
    }

    [Fact(DisplayName = "US-TK-06 | Another user cannot check the task")]
    public async Task Other_user_cannot_check()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.AjaxAsync(HttpMethod.Put, $"/api/tasks/{TestApp.Passport}/check")).StatusCode);
        Assert.False((await app.TaskAsync(TestApp.Passport))!.Checked);
    }

    [Fact(DisplayName = "US-TK-06 | Checking a deleted task is a bad request")]
    public async Task Check_deleted_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.AjaxAsync(HttpMethod.Put, $"/api/tasks/{TestApp.OldTask}/check");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Message\":\"Cannot check a deleted item\"}", await response.Content.ReadAsStringAsync());
    }

    // US-TK-07 Uncheck a task

    [Fact(DisplayName = "US-TK-07 | Unchecking a task clears the check")]
    public async Task Uncheck_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NoContent, (await browser.AjaxAsync(HttpMethod.Delete, $"/api/tasks/{TestApp.Charger}/check")).StatusCode);
        Assert.False((await app.TaskAsync(TestApp.Charger))!.Checked);
    }

    [Fact(DisplayName = "US-TK-07 | Unchecking a missing task is not found")]
    public async Task Uncheck_missing_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.AjaxAsync(HttpMethod.Delete, "/api/tasks/9999/check")).StatusCode);
    }

    [Fact(DisplayName = "US-TK-07 | Another user cannot uncheck the task")]
    public async Task Other_user_cannot_uncheck()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Other);

        Assert.Equal(HttpStatusCode.Unauthorized, (await browser.AjaxAsync(HttpMethod.Delete, $"/api/tasks/{TestApp.Charger}/check")).StatusCode);
        Assert.True((await app.TaskAsync(TestApp.Charger))!.Checked);
    }

    [Fact(DisplayName = "US-TK-07 | Unchecking a deleted task is a bad request")]
    public async Task Uncheck_deleted_task()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.AjaxAsync(HttpMethod.Delete, $"/api/tasks/{TestApp.OldTask}/check");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("{\"Message\":\"Cannot uncheck a deleted item\"}", await response.Content.ReadAsStringAsync());
    }
}
