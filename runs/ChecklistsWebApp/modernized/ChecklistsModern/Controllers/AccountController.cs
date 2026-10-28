using System.Security.Claims;
using ChecklistsModern.Models;
using ChecklistsModern.Services;
using ChecklistsModern.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ChecklistsModern.Controllers;

[Authorize]
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IEmailSender emailSender,
        ISmsSender smsSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
        _smsSender = smsSender;
    }

    //
    // GET: /Account/Login
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    //
    // POST: /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Failed passwords do not count towards lockout, as in the legacy shouldLockout: false.
        var result = await _signInManager.PasswordSignInAsync(model.Email!, model.Password!, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
            return RedirectToLocal(returnUrl);
        if (result.IsLockedOut)
            return View("Lockout");
        if (result.RequiresTwoFactor)
            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });

        ModelState.AddModelError("", "Invalid login attempt.");
        return View(model);
    }

    //
    // GET: /Account/VerifyCode
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCode(string? provider, string? returnUrl, bool rememberMe)
    {
        // Require that the user has already logged in via username/password or external login
        if (await _signInManager.GetTwoFactorAuthenticationUserAsync() == null)
        {
            return View("Error");
        }
        return View(new VerifyCodeViewModel { Provider = provider, ReturnUrl = returnUrl, RememberMe = rememberMe });
    }

    //
    // POST: /Account/VerifyCode
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyCode(VerifyCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _signInManager.TwoFactorSignInAsync(model.Provider!, model.Code!, isPersistent: model.RememberMe, rememberClient: model.RememberBrowser);
        if (result.Succeeded)
            return RedirectToLocal(model.ReturnUrl);
        if (result.IsLockedOut)
            return View("Lockout");

        ModelState.AddModelError("", "Invalid code.");
        return View(model);
    }

    //
    // GET: /Account/Register
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View();
    }

    //
    // POST: /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName!
            };

            var result = await _userManager.CreateAsync(user, model.Password!);
            if (result.Succeeded)
            {
                await _signInManager.SignInAsync(user, isPersistent: false);
                return RedirectToAction("Index", "Checklists");
            }
            AddErrors(result);
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    //
    // GET: /Account/ConfirmEmail
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string? userId, string? code)
    {
        if (userId == null || code == null)
        {
            return View("Error");
        }
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return View("Error");
        }
        var result = await _userManager.ConfirmEmailAsync(user, code);
        return View(result.Succeeded ? "ConfirmEmail" : "Error");
    }

    //
    // GET: /Account/ForgotPassword
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    //
    // POST: /Account/ForgotPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.FindByNameAsync(model.Email!);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return View("ForgotPasswordConfirmation");
            }

            // The legacy app never sent the reset link: that code was commented out, so a confirmed user sees the form again.
        }

        // If we got this far, something failed, redisplay form
        return View(model);
    }

    //
    // GET: /Account/ForgotPasswordConfirmation
    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    //
    // GET: /Account/ResetPassword
    [AllowAnonymous]
    public IActionResult ResetPassword(string? code)
    {
        return code == null ? View("Error") : View();
    }

    //
    // POST: /Account/ResetPassword
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await _userManager.FindByNameAsync(model.Email!);
        if (user == null)
        {
            // Don't reveal that the user does not exist
            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }
        var result = await _userManager.ResetPasswordAsync(user, model.Code ?? string.Empty, model.Password!);
        if (result.Succeeded)
        {
            return RedirectToAction("ResetPasswordConfirmation", "Account");
        }
        AddErrors(result);
        return View();
    }

    //
    // GET: /Account/ResetPasswordConfirmation
    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    //
    // POST: /Account/ExternalLogin
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLogin(string? provider, string? returnUrl)
    {
        if (!await IsExternalProviderAsync(provider))
        {
            // OWIN answered a challenge for an unregistered provider with a 401, which the cookie middleware sent to the login page.
            return Challenge();
        }

        // Request a redirect to the external login provider
        var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider!);
    }

    //
    // GET: /Account/SendCode
    [AllowAnonymous]
    public async Task<IActionResult> SendCode(string? returnUrl, bool rememberMe)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return View("Error");
        }
        var userFactors = await _userManager.GetValidTwoFactorProvidersAsync(user);
        var factorOptions = userFactors.Select(purpose => new SelectListItem { Text = purpose, Value = purpose }).ToList();
        return View(new SendCodeViewModel { Providers = factorOptions, ReturnUrl = returnUrl, RememberMe = rememberMe });
    }

    //
    // POST: /Account/SendCode
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendCode(SendCodeViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        // Generate the token and send it
        if (!await SendTwoFactorCodeAsync(model.SelectedProvider))
        {
            return View("Error");
        }
        return RedirectToAction("VerifyCode", new { Provider = model.SelectedProvider, ReturnUrl = model.ReturnUrl, RememberMe = model.RememberMe });
    }

    //
    // GET: /Account/ExternalLoginCallback
    [AllowAnonymous]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl)
    {
        var loginInfo = await _signInManager.GetExternalLoginInfoAsync();
        if (loginInfo == null)
        {
            return RedirectToAction("Login");
        }

        // Sign in the user with this external login provider if the user already has a login
        var result = await _signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent: false);
        if (result.Succeeded)
            return RedirectToLocal(returnUrl);
        if (result.IsLockedOut)
            return View("Lockout");
        if (result.RequiresTwoFactor)
            return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });

        // If the user does not have an account, then prompt the user to create an account
        ViewBag.ReturnUrl = returnUrl;
        ViewBag.LoginProvider = loginInfo.LoginProvider;
        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { Email = loginInfo.Principal.FindFirstValue(ClaimTypes.Email) });
    }

    //
    // POST: /Account/ExternalLoginConfirmation
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Manage");
        }

        if (ModelState.IsValid)
        {
            // Get the information about the user from the external login provider
            var info = await _signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                return View("ExternalLoginFailure");
            }
            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                result = await _userManager.AddLoginAsync(user, info);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToLocal(returnUrl);
                }
            }
            AddErrors(result);
        }

        ViewBag.ReturnUrl = returnUrl;
        return View(model);
    }

    //
    // POST: /Account/LogOff
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOff()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Checklists");
    }

    //
    // GET: /Account/ExternalLoginFailure
    [AllowAnonymous]
    public IActionResult ExternalLoginFailure()
    {
        return View();
    }

    #region Helpers

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        // The legacy route table generated /Home for Home/Index even though no HomeController exists.
        return LocalRedirect("~/Home");
    }

    private async Task<bool> IsExternalProviderAsync(string? provider)
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        return schemes.Any(s => s.Name == provider);
    }

    // Port of SignInManager.SendTwoFactorCodeAsync from Identity 2, which ASP.NET Core Identity does not have.
    private async Task<bool> SendTwoFactorCodeAsync(string? provider)
    {
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return false;
        }
        var token = await _userManager.GenerateTwoFactorTokenAsync(user, provider!);
        if (provider == "Email Code")
        {
            await _emailSender.SendAsync(user.Email, "Security Code", $"Your security code is {token}");
        }
        else if (provider == "Phone Code")
        {
            await _smsSender.SendAsync(user.PhoneNumber, $"Your security code is {token}");
        }
        return true;
    }

    #endregion
}
