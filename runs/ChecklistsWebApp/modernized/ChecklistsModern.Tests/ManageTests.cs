using System.Net;
using ChecklistsModern.Services;
using ChecklistsModern.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ChecklistsModern.Tests;

public class ManageTests
{
    private static readonly Dictionary<string, string?> NoFields = new();

    // US-MG-01 Account settings page

    [Fact(DisplayName = "US-MG-01 | The settings page shows password, external logins, and two-factor sections")]
    public async Task Settings_page_sections()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/Manage");

        Assert.Contains("<h2>My Account.</h2>", html);
        Assert.Contains("Change your account settings", html);
        Assert.Contains("<dt>Password:</dt>", html);
        Assert.Contains("<a href=\"/Manage/ChangePassword\">Change your password</a>", html);
        Assert.Contains("<dt>External Logins:</dt>", html);
        Assert.Matches(@"0 \[\s*<a href=""/Manage/ManageLogins"">Manage</a>", html);
        Assert.Contains("<dt>Two-Factor Authentication:</dt>", html);
        Assert.Contains("There are no two-factor authentication providers configured.", html);
    }

    [Fact(DisplayName = "US-MG-01 | A status message is shown for each message id")]
    public async Task Status_message_per_id()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var expected = new Dictionary<string, string>
        {
            ["AddPhoneSuccess"] = "Your phone number was added.",
            ["RemovePhoneSuccess"] = "Your phone number was removed.",
            ["Error"] = "An error has occurred.",
            ["ChangePasswordSuccess"] = "Your password has been changed.",
            ["SetPasswordSuccess"] = "Your password has been set.",
            ["SetTwoFactorSuccess"] = "Your two-factor authentication provider has been set."
        };

        foreach (var (id, text) in expected)
            Assert.Contains($"<p class=\"text-success\">{text}</p>", await browser.GetHtmlAsync($"/Manage?message={id}"));
    }

    [Fact(DisplayName = "US-MG-01 | The settings page requires a signed-in user")]
    public async Task Settings_page_requires_sign_in()
    {
        using var app = new TestApp();

        (await app.NewBrowser().GetAsync("/Manage")).AssertRedirect("/Account/Login?ReturnUrl=%2FManage");
    }

    // US-MG-02 Change password

    private static Dictionary<string, string?> ChangePassword(string oldPassword, string newPassword, string? confirm = null) =>
        new() { ["OldPassword"] = oldPassword, ["NewPassword"] = newPassword, ["ConfirmPassword"] = confirm ?? newPassword };

    [Fact(DisplayName = "US-MG-02 | The change password page shows the three password fields")]
    public async Task Change_password_page_fields()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/Manage/ChangePassword");

        Assert.Contains("<h2>Change Password</h2>", html);
        foreach (var label in new[] { "Current password", "New password", "Confirm new password" })
            Assert.Contains($">{label}</label>", html);
        foreach (var name in new[] { "OldPassword", "NewPassword", "ConfirmPassword" })
            Assert.Contains($"name=\"{name}\"", html);
        Assert.Contains("value=\"Change password\"", html);
    }

    [Fact(DisplayName = "US-MG-02 | A correct change saves the new password and returns to the list")]
    public async Task Correct_change_password()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/ChangePassword", ChangePassword(TestApp.Password, "Changed1!"), tokenPage: "/");

        response.AssertRedirect("/");
        (await app.NewBrowser().SignInAsync(TestApp.Author, "Changed1!")).AssertRedirect("/");
    }

    [Fact(DisplayName = "US-MG-02 | A wrong current password shows Incorrect password")]
    public async Task Wrong_current_password()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/ChangePassword", ChangePassword("wrong", "Changed1!"), tokenPage: "/");

        Assert.Contains("Incorrect password.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-MG-02 | A change password confirmation that does not match shows the compare message")]
    public async Task Change_password_confirmation_mismatch()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/ChangePassword", ChangePassword(TestApp.Password, "Changed1!", "Changed2!"), tokenPage: "/");

        Assert.Contains("The new password and confirmation password do not match.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-MG-02 | A new password shorter than 6 characters shows the length message")]
    public async Task Change_password_too_short()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/ChangePassword", ChangePassword(TestApp.Password, "Ab1!"), tokenPage: "/");

        Assert.Contains("The New password must be at least 6 characters long.", await response.HtmlAsync());
    }

    // US-MG-03 Set password

    [Fact(DisplayName = "US-MG-03 | The set password page shows the local login form")]
    public async Task Set_password_page()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/Manage/SetPassword");

        Assert.Contains("<h2>Create Password.</h2>", html);
        Assert.Contains("Create Local Login", html);
        Assert.Contains(">New password</label>", html);
        Assert.Contains(">Confirm new password</label>", html);
        Assert.Contains("value=\"Set password\"", html);
    }

    [Fact(DisplayName = "US-MG-03 | A user who already has a password cannot set another one")]
    public async Task Set_password_when_one_exists()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/SetPassword",
            new Dictionary<string, string?> { ["NewPassword"] = "Another1!", ["ConfirmPassword"] = "Another1!" }, tokenPage: "/");

        Assert.Contains("User already has a password set.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-MG-03 | A set password confirmation that does not match shows the compare message")]
    public async Task Set_password_confirmation_mismatch()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/SetPassword",
            new Dictionary<string, string?> { ["NewPassword"] = "Another1!", ["ConfirmPassword"] = "Another2!" }, tokenPage: "/");

        Assert.Contains("The new password and confirmation password do not match.", await response.HtmlAsync());
    }

    // US-MG-04 Manage external logins

    [Fact(DisplayName = "US-MG-04 | With no provider configured the page says so")]
    public async Task Manage_logins_without_providers()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/Manage/ManageLogins");

        Assert.Contains("Manage your external logins.", html);
        Assert.Contains("There are no external authentication services configured.", html);
    }

    [Fact(DisplayName = "US-MG-04 | The error message id shows the error text")]
    public async Task Manage_logins_error_message()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Contains("<p class=\"text-success\">An error has occurred.</p>", await browser.GetHtmlAsync("/Manage/ManageLogins?message=Error"));
    }

    [Fact(DisplayName = "US-MG-04 | Removing a login the user does not have goes back to the logins page")]
    public async Task Remove_missing_login()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/RemoveLogin",
            new Dictionary<string, string?> { ["loginProvider"] = "Google", ["providerKey"] = "x" }, tokenPage: "/");

        response.AssertRedirect("/Manage/ManageLogins?Message=RemoveLoginSuccess");
    }

    [Fact(DisplayName = "US-MG-04 | The link login callback with no external sign-in goes back with the error message")]
    public async Task Link_login_callback_without_external_sign_in()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        (await browser.GetAsync("/Manage/LinkLoginCallback")).AssertRedirect("/Manage/ManageLogins?Message=Error");
    }

    [Fact(DisplayName = "US-MG-04 | Linking a provider that is not configured does not add a login")]
    public async Task Link_unconfigured_provider()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/LinkLogin", new Dictionary<string, string?> { ["provider"] = "Google" }, tokenPage: "/");

        Assert.Equal(HttpStatusCode.Found, response.StatusCode);
        var logins = await app.UsersAsync(async users => await users.GetLoginsAsync((await users.FindByEmailAsync(TestApp.Author))!));
        Assert.Empty(logins);
    }

    // US-MG-05 Phone number

    [Fact(DisplayName = "US-MG-05 | The add phone page shows the number field")]
    public async Task Add_phone_page()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/Manage/AddPhoneNumber");

        Assert.Contains("<h2>Phone Number.</h2>", html);
        Assert.Contains("Add a phone number", html);
        Assert.Contains(">Phone Number</label>", html);
        Assert.Contains("name=\"Number\"", html);
        Assert.Contains("value=\"Submit\"", html);
    }

    [Fact(DisplayName = "US-MG-05 | A valid number goes to the verify page without sending a text")]
    public async Task Valid_number_goes_to_verify()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/AddPhoneNumber", new Dictionary<string, string?> { ["Number"] = "5551234567" }, tokenPage: "/");

        response.AssertRedirect("/Manage/VerifyPhoneNumber?PhoneNumber=5551234567");
        Assert.IsType<NoOpSmsSender>(app.Services.GetRequiredService<ISmsSender>());
    }

    [Fact(DisplayName = "US-MG-05 | An invalid number shows the phone message")]
    public async Task Invalid_number_shows_message()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/AddPhoneNumber", new Dictionary<string, string?> { ["Number"] = "abc" }, tokenPage: "/");

        Assert.Contains("The Phone Number field is not a valid phone number.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-MG-05 | The verify page without a number shows the error page")]
    public async Task Verify_page_without_number()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Contains("An error occurred while processing your request.", await browser.GetHtmlAsync("/Manage/VerifyPhoneNumber"));
    }

    [Fact(DisplayName = "US-MG-05 | The verify page with a number shows the code form")]
    public async Task Verify_page_with_number()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var html = await browser.GetHtmlAsync("/Manage/VerifyPhoneNumber?phoneNumber=5551234567");

        Assert.Contains("<h2>Verify Phone Number.</h2>", html);
        Assert.Contains("Enter verification code", html);
        Assert.Contains(">Code</label>", html);
        Assert.Contains("value=\"Submit\"", html);
        Assert.Contains("name=\"phoneNumber\" type=\"hidden\" value=\"5551234567\"", html);
    }

    [Fact(DisplayName = "US-MG-05 | A wrong code shows Failed to verify phone")]
    public async Task Wrong_phone_code()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/VerifyPhoneNumber",
            new Dictionary<string, string?> { ["phoneNumber"] = "5551234567", ["Code"] = "000000" }, tokenPage: "/");

        Assert.Contains("Failed to verify phone", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-MG-05 | A correct code saves the number")]
    public async Task Correct_phone_code()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);
        var code = await app.UsersAsync(async users => await users.GenerateChangePhoneNumberTokenAsync((await users.FindByEmailAsync(TestApp.Author))!, "5551234567"));

        var response = await browser.PostFormAsync("/Manage/VerifyPhoneNumber",
            new Dictionary<string, string?> { ["phoneNumber"] = "5551234567", ["Code"] = code }, tokenPage: "/");

        response.AssertRedirect("/Manage?Message=AddPhoneSuccess");
        Assert.Equal("5551234567", (await app.UserAsync(TestApp.Author)).PhoneNumber);
    }

    [Fact(DisplayName = "US-MG-05 | Remove phone number clears the number")]
    public async Task Remove_phone_number()
    {
        using var app = new TestApp();
        await app.UsersAsync(async users => await users.SetPhoneNumberAsync((await users.FindByEmailAsync(TestApp.Author))!, "5551234567"));
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/RemovePhoneNumber", NoFields, tokenPage: "/");

        response.AssertRedirect("/Manage?Message=RemovePhoneSuccess");
        Assert.Null((await app.UserAsync(TestApp.Author)).PhoneNumber);
    }

    // US-MG-06 Turn two-factor on and off

    [Fact(DisplayName = "US-MG-06 | Enable two-factor turns it on and returns to Manage")]
    public async Task Enable_two_factor()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/EnableTwoFactorAuthentication", NoFields, tokenPage: "/");

        response.AssertRedirect("/Manage");
        Assert.True((await app.UserAsync(TestApp.Author)).TwoFactorEnabled);
    }

    [Fact(DisplayName = "US-MG-06 | Disable two-factor turns it off and returns to Manage")]
    public async Task Disable_two_factor()
    {
        using var app = new TestApp();
        await app.UsersAsync(async users => await users.SetTwoFactorEnabledAsync((await users.FindByEmailAsync(TestApp.Author))!, true));
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Manage/DisableTwoFactorAuthentication", NoFields, tokenPage: "/");

        response.AssertRedirect("/Manage");
        Assert.False((await app.UserAsync(TestApp.Author)).TwoFactorEnabled);
    }

    [Fact(DisplayName = "US-MG-06 | Enable two-factor without the verification token is rejected")]
    public async Task Enable_two_factor_without_token()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormWithoutTokenAsync("/Manage/EnableTwoFactorAuthentication", NoFields);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False((await app.UserAsync(TestApp.Author)).TwoFactorEnabled);
    }
}
