using System.Net;
using System.Text.RegularExpressions;
using ChecklistsModern.Tests.Infrastructure;

namespace ChecklistsModern.Tests;

public class SharedFoundationTests
{
    [Fact(DisplayName = "US-SF-01 | Every page shows the brand, the page title, and the footer")]
    public async Task Every_page_shows_brand_title_footer()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/Login");

        Assert.Contains("<title>Log in - Checklists</title>", html);
        Assert.Matches(new Regex("<a href=\"/\" class=\"navbar-brand\">\\s*<img src=\"/Content/Images/checklist_logo.png\" /> Checklists"), html);
        Assert.Contains($"© {DateTime.Now.Year} - Checklists", html);
    }

    [Fact(DisplayName = "US-SF-01 | An anonymous visitor sees Register and Log in in the navbar")]
    public async Task Anonymous_sees_register_and_login()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/Login");

        Assert.Contains("<a href=\"/Account/Register\" id=\"registerLink\">Register</a>", html);
        Assert.Contains("<a href=\"/Account/Login\" id=\"loginLink\">Log in</a>", html);
        Assert.DoesNotContain("logoutForm", html);
    }

    [Fact(DisplayName = "US-SF-01 | A signed-in user sees their full name, Change Password, and Log out")]
    public async Task Signed_in_user_sees_name_and_menu()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/");

        Assert.Contains("Hello Alex Author!", html);
        Assert.Contains("<a href=\"/Manage/ChangePassword\">Change Password</a>", html);
        Assert.Contains("Log out</a>", html);
        Assert.DoesNotContain("registerLink", html);
    }

    [Fact(DisplayName = "US-SF-01 | Pages load the legacy stylesheets")]
    public async Task Pages_load_legacy_stylesheets()
    {
        using var app = new TestApp();
        var browser = app.NewBrowser();
        var html = await browser.GetHtmlAsync("/Account/Login");

        var hrefs = Regex.Matches(html, "<link href=\"([^\"]+)\" rel=\"stylesheet\"").Select(m => m.Groups[1].Value).ToList();
        Assert.Equal(new[] { "/Content/bootstrap.css", "/Content/bootstrap-paper.css", "/Content/Site.css" }, hrefs);
        foreach (var href in hrefs)
        {
            var css = await browser.GetAsync(href);
            Assert.Equal(HttpStatusCode.OK, css.StatusCode);
            Assert.True((await css.Content.ReadAsByteArrayAsync()).Length > 0, $"{href} is empty");
        }
    }

    [Fact(DisplayName = "US-SF-02 | The root URL shows the checklist list")]
    public async Task Root_shows_checklist_list()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Contains("My Checklists", await browser.GetHtmlAsync("/"));
    }

    [Fact(DisplayName = "US-SF-02 | The Home URL does not exist")]
    public async Task Home_url_does_not_exist()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/Home")).StatusCode);
    }

    [Fact(DisplayName = "US-SF-03 | A flow that returns the Error view shows the error text")]
    public async Task Error_view_shows_error_text()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/ConfirmEmail");

        Assert.Contains("Error.", html);
        Assert.Contains("An error occurred while processing your request.", html);
    }

    [Fact(DisplayName = "US-SF-03 | The error page uses the layout")]
    public async Task Error_page_uses_layout()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/ConfirmEmail");

        Assert.Contains("<title>Error - Checklists</title>", html);
        Assert.Contains("navbar-brand", html);
    }
}
