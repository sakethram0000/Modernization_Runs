using System.Net;
using ChecklistsModern.Services;
using ChecklistsModern.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace ChecklistsModern.Tests;

public class AccountTests
{
    private static Dictionary<string, string?> Login(string email, string? password) =>
        new() { ["Email"] = email, ["Password"] = password, ["RememberMe"] = "false" };

    private static Dictionary<string, string?> Registration(string fullName, string email, string password, string? confirm = null) =>
        new() { ["FullName"] = fullName, ["Email"] = email, ["Password"] = password, ["ConfirmPassword"] = confirm ?? password };

    // US-AC-01 Log in

    [Fact(DisplayName = "US-AC-01 | The login page shows the email, password, and remember me fields")]
    public async Task Login_page_shows_fields()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/Login");

        Assert.Contains("<h2>Log in.</h2>", html);
        Assert.Contains("<label for=\"Email\">Email</label>", html);
        Assert.Contains("<label for=\"Password\">Password</label>", html);
        Assert.Contains("<label for=\"RememberMe\">Remember me?</label>", html);
        Assert.Contains("name=\"Email\"", html);
        Assert.Contains("name=\"Password\"", html);
        Assert.Contains("name=\"RememberMe\"", html);
        Assert.Contains("value=\"Log in\"", html);
        Assert.Contains("<a href=\"/Account/Register\">Create an account</a>", html);
    }

    [Fact(DisplayName = "US-AC-01 | A correct login with a local return URL goes back to that URL")]
    public async Task Correct_login_with_local_return_url()
    {
        using var app = new TestApp();
        var browser = app.NewBrowser();

        var response = await browser.SignInAsync(TestApp.Author, returnUrl: "/");

        response.AssertRedirect("/");
        Assert.Contains("My Checklists", await browser.GetHtmlAsync("/"));
    }

    [Fact(DisplayName = "US-AC-01 | A correct login with no return URL goes to /Home")]
    public async Task Correct_login_without_return_url_goes_home()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().SignInAsync(TestApp.Author, returnUrl: null);

        response.AssertRedirect("/Home");
    }

    [Fact(DisplayName = "US-AC-01 | A login with a non-local return URL goes to /Home")]
    public async Task Login_with_non_local_return_url_goes_home()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().SignInAsync(TestApp.Author, returnUrl: "http://example.com/");

        response.AssertRedirect("/Home");
    }

    [Fact(DisplayName = "US-AC-01 | A wrong password shows Invalid login attempt")]
    public async Task Wrong_password_shows_invalid_login()
    {
        using var app = new TestApp();
        var browser = app.NewBrowser();

        var response = await browser.SignInAsync(TestApp.Author, "Wrong123!");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Invalid login attempt.", await response.HtmlAsync());
        (await browser.GetAsync("/")).AssertRedirectToLogin();
    }

    [Fact(DisplayName = "US-AC-01 | An empty email and password show the required messages")]
    public async Task Empty_login_shows_required_messages()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/Login", Login("", ""));
        var html = await response.HtmlAsync();

        Assert.Contains("The Email field is required.", html);
        Assert.Contains("The Password field is required.", html);
    }

    [Fact(DisplayName = "US-AC-01 | An email that is not an email address shows the format message")]
    public async Task Bad_email_shows_format_message()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/Login", Login("not-an-email", "x"));

        Assert.Contains("The Email field is not a valid e-mail address.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-AC-01 | Login without the verification token is rejected")]
    public async Task Login_without_token_is_rejected()
    {
        using var app = new TestApp();
        var browser = app.NewBrowser();

        var response = await browser.PostFormWithoutTokenAsync("/Account/Login", Login(TestApp.Author, TestApp.Password));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        (await browser.GetAsync("/")).AssertRedirectToLogin();
    }

    [Fact(DisplayName = "US-AC-01 | A locked-out user sees the lockout page")]
    public async Task Locked_out_user_sees_lockout_page()
    {
        using var app = new TestApp();
        await app.UsersAsync(async users =>
            await users.SetLockoutEndDateAsync((await users.FindByEmailAsync(TestApp.Author))!, DateTimeOffset.UtcNow.AddHours(1)));

        var response = await app.NewBrowser().SignInAsync(TestApp.Author);
        var html = await response.HtmlAsync();

        Assert.Contains("Locked out.", html);
        Assert.Contains("This account has been locked out, please try again later.", html);
    }

    [Fact(DisplayName = "US-AC-01 | Wrong passwords never lock the account")]
    public async Task Wrong_passwords_never_lock()
    {
        using var app = new TestApp();
        var browser = app.NewBrowser();
        for (var i = 0; i < 6; i++)
            await browser.SignInAsync(TestApp.Author, "Wrong123!");

        var response = await browser.SignInAsync(TestApp.Author);

        response.AssertRedirect("/");
        Assert.Contains("Hello Alex Author!", await browser.GetHtmlAsync("/"));
    }

    [Fact(DisplayName = "US-AC-01 | A user with two-factor enabled is sent to choose a code provider")]
    public async Task Two_factor_user_goes_to_send_code()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().SignInAsync(TestApp.TwoFactor, returnUrl: "/");

        response.AssertRedirect("/Account/SendCode?ReturnUrl=%2F&RememberMe=False");
    }

    // US-AC-02 Register

    [Fact(DisplayName = "US-AC-02 | The register page shows the account fields")]
    public async Task Register_page_shows_fields()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/Register");

        Assert.Contains("<h2>Create an account.</h2>", html);
        foreach (var label in new[] { "Full name", "Email", "Password", "Confirm password" })
            Assert.Contains($">{label}</label>", html);
        foreach (var name in new[] { "FullName", "Email", "Password", "ConfirmPassword" })
            Assert.Contains($"name=\"{name}\"", html);
        Assert.Contains("value=\"Create account\"", html);
    }

    [Fact(DisplayName = "US-AC-02 | A valid registration creates the user, signs them in, and opens the list")]
    public async Task Valid_registration_signs_in()
    {
        using var app = new TestApp();
        var browser = app.NewBrowser();

        var response = await browser.PostFormAsync("/Account/Register", Registration("Nina New", "nina@checklists.local", "Strong1!"));

        response.AssertRedirect("/");
        var html = await browser.GetHtmlAsync("/");
        Assert.Contains("Hello Nina New!", html);
        Assert.Contains("You don't have any checklists.", html);
        var user = await app.UserAsync("nina@checklists.local");
        Assert.Equal("Nina New", user.FullName);
        Assert.Equal("nina@checklists.local", user.UserName);
    }

    [Fact(DisplayName = "US-AC-02 | Registering an email that already exists shows the duplicate messages")]
    public async Task Duplicate_email_shows_messages()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/Register", Registration("Again", TestApp.Author, "Strong1!"));
        var html = await response.HtmlAsync();

        Assert.Contains("Name author@checklists.local is already taken.", html);
        Assert.Contains("Email 'author@checklists.local' is already taken.", html);
    }

    [Fact(DisplayName = "US-AC-02 | A password that breaks the rules shows the password message")]
    public async Task Weak_password_shows_rules_message()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/Register", Registration("Nina New", "nina@checklists.local", "abcdef"));

        Assert.Contains("Passwords must have at least one non letter or digit character. Passwords must have at least one digit ('0'-'9'). Passwords must have at least one uppercase ('A'-'Z').",
            await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-AC-02 | A password shorter than 6 characters shows the length message")]
    public async Task Short_password_shows_length_message()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/Register", Registration("Nina New", "nina@checklists.local", "Ab1!"));

        Assert.Contains("The Password must be at least 6 characters long.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-AC-02 | A registration confirmation that does not match shows the compare message")]
    public async Task Registration_confirmation_mismatch()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/Register", Registration("Nina New", "nina@checklists.local", "Strong1!", "Strong2!"));

        Assert.Contains("The password and confirmation password do not match.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-AC-02 | A missing full name shows the required message")]
    public async Task Missing_full_name_shows_required()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/Register", Registration("", "nina@checklists.local", "Strong1!"));

        Assert.Contains("The Full name field is required.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-AC-02 | A full name longer than 255 characters shows the length message")]
    public async Task Long_full_name_shows_length()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/Register", Registration(new string('a', 256), "nina@checklists.local", "Strong1!"));

        Assert.Contains("The field Full name must be a string with a maximum length of 255.", await response.HtmlAsync());
        Assert.Null(await app.UsersAsync(users => users.FindByEmailAsync("nina@checklists.local")));
    }

    // US-AC-03 Log out

    [Fact(DisplayName = "US-AC-03 | Log out signs me out and returns to the list URL")]
    public async Task Log_out_signs_out()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Account/LogOff", new Dictionary<string, string?>(), tokenPage: "/");

        response.AssertRedirect("/");
        (await browser.GetAsync("/")).AssertRedirectToLogin();
    }

    [Fact(DisplayName = "US-AC-03 | Log out is POST only")]
    public async Task Log_out_is_post_only()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/Account/LogOff")).StatusCode);
    }

    [Fact(DisplayName = "US-AC-03 | Log out requires a signed-in user")]
    public async Task Log_out_requires_sign_in()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/LogOff", new Dictionary<string, string?>());

        response.AssertRedirectToLogin();
    }

    // US-AC-04 Forgot password

    [Fact(DisplayName = "US-AC-04 | The forgot password page shows the email field")]
    public async Task Forgot_password_page_fields()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/ForgotPassword");

        Assert.Contains("<h2>Forgot your password?.</h2>", html);
        Assert.Contains("Enter your email.", html);
        Assert.Contains(">Email</label>", html);
        Assert.Contains("name=\"Email\"", html);
        Assert.Contains("value=\"Email Link\"", html);
    }

    [Fact(DisplayName = "US-AC-04 | An unknown email shows the confirmation page")]
    public async Task Forgot_password_unknown_email()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/ForgotPassword", new Dictionary<string, string?> { ["Email"] = "nobody@checklists.local" });
        var html = await response.HtmlAsync();

        Assert.Contains("Forgot Password Confirmation.", html);
        Assert.Contains("Please check your email to reset your password.", html);
    }

    [Fact(DisplayName = "US-AC-04 | An email that is not confirmed shows the confirmation page")]
    public async Task Forgot_password_unconfirmed_email()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/ForgotPassword", new Dictionary<string, string?> { ["Email"] = TestApp.Author });

        Assert.Contains("Please check your email to reset your password.", await response.HtmlAsync());
        Assert.IsType<NoOpEmailSender>(app.Services.GetRequiredService<IEmailSender>());
    }

    [Fact(DisplayName = "US-AC-04 | A confirmed email shows the forgot password form again")]
    public async Task Forgot_password_confirmed_email_shows_form()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/ForgotPassword", new Dictionary<string, string?> { ["Email"] = TestApp.TwoFactor });
        var html = await response.HtmlAsync();

        Assert.Contains("Enter your email.", html);
        Assert.Contains("value=\"Email Link\"", html);
        Assert.DoesNotContain("Please check your email", html);
        Assert.IsType<NoOpEmailSender>(app.Services.GetRequiredService<IEmailSender>());
    }

    [Fact(DisplayName = "US-AC-04 | An invalid email shows the format message")]
    public async Task Forgot_password_invalid_email()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/ForgotPassword", new Dictionary<string, string?> { ["Email"] = "not-an-email" });

        Assert.Contains("The Email field is not a valid e-mail address.", await response.HtmlAsync());
    }

    // US-AC-05 Reset password

    private static Dictionary<string, string?> Reset(string email, string code, string password) =>
        new() { ["Email"] = email, ["Code"] = code, ["Password"] = password, ["ConfirmPassword"] = password };

    [Fact(DisplayName = "US-AC-05 | The reset page without a code shows the error page")]
    public async Task Reset_without_code_shows_error()
    {
        using var app = new TestApp();

        Assert.Contains("An error occurred while processing your request.", await app.NewBrowser().GetHtmlAsync("/Account/ResetPassword"));
    }

    [Fact(DisplayName = "US-AC-05 | The reset page with a code shows the reset form")]
    public async Task Reset_with_code_shows_form()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/ResetPassword?code=abc");

        Assert.Contains("<h2>Reset password.</h2>", html);
        Assert.Contains("Reset your password.", html);
        foreach (var label in new[] { "Email", "Password", "Confirm password" })
            Assert.Contains($">{label}</label>", html);
        Assert.Contains("value=\"Reset\"", html);
    }

    [Fact(DisplayName = "US-AC-05 | A reset for an unknown email goes to the confirmation page")]
    public async Task Reset_unknown_email_goes_to_confirmation()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/ResetPassword", Reset("nobody@checklists.local", "abc", "Changed1!"));

        response.AssertRedirect("/Account/ResetPasswordConfirmation");
    }

    [Fact(DisplayName = "US-AC-05 | A reset with a bad code shows Invalid token")]
    public async Task Reset_bad_code_shows_invalid_token()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/ResetPassword", Reset(TestApp.Author, "bad", "Changed1!"));

        Assert.Contains("Invalid token.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-AC-05 | A reset with a valid code changes the password")]
    public async Task Reset_valid_code_changes_password()
    {
        using var app = new TestApp();
        var code = await app.UsersAsync(async users => await users.GeneratePasswordResetTokenAsync((await users.FindByEmailAsync(TestApp.Author))!));

        var response = await app.NewBrowser().PostFormAsync("/Account/ResetPassword", Reset(TestApp.Author, code, "Changed1!"));

        response.AssertRedirect("/Account/ResetPasswordConfirmation");
        (await app.NewBrowser().SignInAsync(TestApp.Author, "Changed1!")).AssertRedirect("/");
    }

    [Fact(DisplayName = "US-AC-05 | The reset confirmation page links to the login page")]
    public async Task Reset_confirmation_links_to_login()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/ResetPasswordConfirmation");

        Assert.Contains("Reset password confirmation.", html);
        Assert.Contains("Your password has been reset. Please", html);
        Assert.Contains("<a href=\"/Account/Login\" id=\"loginLink\">click here to log in</a>", html);
    }

    // US-AC-06 Confirm email

    [Fact(DisplayName = "US-AC-06 | Confirm email with a missing user id or code shows the error page")]
    public async Task Confirm_email_missing_values_shows_error()
    {
        using var app = new TestApp();

        Assert.Contains("An error occurred while processing your request.", await app.NewBrowser().GetHtmlAsync("/Account/ConfirmEmail?userId=x"));
    }

    [Fact(DisplayName = "US-AC-06 | Confirm email with a bad code shows the error page")]
    public async Task Confirm_email_bad_code_shows_error()
    {
        using var app = new TestApp();
        var author = await app.UserAsync(TestApp.Author);

        var html = await app.NewBrowser().GetHtmlAsync($"/Account/ConfirmEmail?userId={author.Id}&code=bad");

        Assert.Contains("An error occurred while processing your request.", html);
    }

    [Fact(DisplayName = "US-AC-06 | Confirm email with a valid code confirms the email")]
    public async Task Confirm_email_valid_code()
    {
        using var app = new TestApp();
        var author = await app.UserAsync(TestApp.Author);
        var code = await app.UsersAsync(async users => await users.GenerateEmailConfirmationTokenAsync((await users.FindByIdAsync(author.Id))!));

        var html = await app.NewBrowser().GetHtmlAsync($"/Account/ConfirmEmail?userId={author.Id}&code={Uri.EscapeDataString(code)}");

        Assert.Contains("Confirm Email.", html);
        Assert.Contains("Thank you for confirming your email. Please", html);
        Assert.Contains(">Click here to Log in</a>", html);
        Assert.True((await app.UserAsync(TestApp.Author)).EmailConfirmed);
    }

    // US-AC-07 Two-factor sign-in

    private static async Task<Browser> PassPasswordStepAsync(TestApp app)
    {
        var browser = app.NewBrowser();
        (await browser.SignInAsync(TestApp.TwoFactor, returnUrl: "/")).AssertRedirect("/Account/SendCode?ReturnUrl=%2F&RememberMe=False");
        return browser;
    }

    private static Dictionary<string, string?> VerifyCode(string code) =>
        new() { ["Provider"] = "Email Code", ["Code"] = code, ["ReturnUrl"] = "/", ["RememberMe"] = "False", ["RememberBrowser"] = "false" };

    [Fact(DisplayName = "US-AC-07 | Send code without a password step shows the error page")]
    public async Task Send_code_without_password_step()
    {
        using var app = new TestApp();

        Assert.Contains("An error occurred while processing your request.", await app.NewBrowser().GetHtmlAsync("/Account/SendCode?rememberMe=false"));
    }

    [Fact(DisplayName = "US-AC-07 | Verify code without a password step shows the error page")]
    public async Task Verify_code_without_password_step()
    {
        using var app = new TestApp();

        Assert.Contains("An error occurred while processing your request.",
            await app.NewBrowser().GetHtmlAsync("/Account/VerifyCode?provider=Email%20Code&rememberMe=false"));
    }

    [Fact(DisplayName = "US-AC-07 | After the password step the send code page lists the providers")]
    public async Task Send_code_lists_providers()
    {
        using var app = new TestApp();
        var browser = await PassPasswordStepAsync(app);

        var html = await browser.GetHtmlAsync("/Account/SendCode?ReturnUrl=%2F&RememberMe=False");

        Assert.Contains("<h2>Send.</h2>", html);
        Assert.Contains("Send verification code", html);
        Assert.Contains("<option value=\"Email Code\">Email Code</option>", html);
        Assert.Contains("value=\"Submit\"", html);
    }

    [Fact(DisplayName = "US-AC-07 | Choosing a provider goes to the verify code page")]
    public async Task Choosing_provider_goes_to_verify()
    {
        using var app = new TestApp();
        var browser = await PassPasswordStepAsync(app);

        var response = await browser.PostFormAsync("/Account/SendCode?ReturnUrl=%2F",
            new Dictionary<string, string?> { ["SelectedProvider"] = "Email Code", ["rememberMe"] = "False" });

        response.AssertRedirect("/Account/VerifyCode?Provider=Email%20Code&ReturnUrl=%2F&RememberMe=False");
    }

    [Fact(DisplayName = "US-AC-07 | A wrong code shows Invalid code")]
    public async Task Wrong_code_shows_invalid_code()
    {
        using var app = new TestApp();
        var browser = await PassPasswordStepAsync(app);

        var response = await browser.PostFormAsync("/Account/VerifyCode", VerifyCode("000000"));

        Assert.Contains("Invalid code.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-AC-07 | A correct code signs the user in")]
    public async Task Correct_code_signs_in()
    {
        using var app = new TestApp();
        var browser = await PassPasswordStepAsync(app);
        var code = await app.UsersAsync(async users => await users.GenerateTwoFactorTokenAsync((await users.FindByEmailAsync(TestApp.TwoFactor))!, "Email Code"));

        var response = await browser.PostFormAsync("/Account/VerifyCode", VerifyCode(code));

        response.AssertRedirect("/");
        Assert.Contains("My Checklists", await browser.GetHtmlAsync("/"));
    }

    // US-AC-08 External logins

    [Fact(DisplayName = "US-AC-08 | The external login failure page shows its message")]
    public async Task External_login_failure_page()
    {
        using var app = new TestApp();
        var html = await app.NewBrowser().GetHtmlAsync("/Account/ExternalLoginFailure");

        Assert.Contains("Login Failure.", html);
        Assert.Contains("Unsuccessful login with service.", html);
    }

    [Fact(DisplayName = "US-AC-08 | The external login callback with no external sign-in goes back to login")]
    public async Task External_callback_without_sign_in()
    {
        using var app = new TestApp();

        (await app.NewBrowser().GetAsync("/Account/ExternalLoginCallback")).AssertRedirect("/Account/Login");
    }

    [Fact(DisplayName = "US-AC-08 | External login confirmation when already signed in goes to Manage")]
    public async Task External_confirmation_when_signed_in()
    {
        using var app = new TestApp();
        var browser = await app.SignedInAsync(TestApp.Author);

        var response = await browser.PostFormAsync("/Account/ExternalLoginConfirmation", new Dictionary<string, string?> { ["Email"] = "x@checklists.local" }, tokenPage: "/");

        response.AssertRedirect("/Manage");
    }

    [Fact(DisplayName = "US-AC-08 | External login confirmation with no external sign-in shows the failure page")]
    public async Task External_confirmation_without_external_sign_in()
    {
        using var app = new TestApp();

        var response = await app.NewBrowser().PostFormAsync("/Account/ExternalLoginConfirmation", new Dictionary<string, string?> { ["Email"] = "x@checklists.local" });

        Assert.Contains("Unsuccessful login with service.", await response.HtmlAsync());
    }

    [Fact(DisplayName = "US-AC-08 | External login with a provider that is not configured does not sign anyone in")]
    public async Task External_login_unconfigured_provider()
    {
        using var app = new TestApp();
        var browser = app.NewBrowser();

        var response = await browser.PostFormAsync("/Account/ExternalLogin", new Dictionary<string, string?> { ["provider"] = "Google" });

        response.AssertRedirectToLogin();
        (await browser.GetAsync("/")).AssertRedirectToLogin();
    }
}
