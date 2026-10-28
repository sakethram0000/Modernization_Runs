using ChecklistsModern.Models;
using ChecklistsModern.Services;
using ChecklistsModern.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChecklistsModern.Controllers;

[Authorize]
public class ManageController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ISmsSender _smsSender;

    public ManageController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ISmsSender smsSender)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _smsSender = smsSender;
    }

    //
    // GET: /Manage/Index
    public async Task<IActionResult> Index(ManageMessageId? message)
    {
        ViewBag.StatusMessage =
            message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
            : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
            : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
            : message == ManageMessageId.Error ? "An error has occurred."
            : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
            : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
            : "";

        var user = await CurrentUserAsync();
        var model = new IndexViewModel
        {
            HasPassword = user.PasswordHash != null,
            PhoneNumber = await _userManager.GetPhoneNumberAsync(user),
            TwoFactor = await _userManager.GetTwoFactorEnabledAsync(user),
            Logins = await _userManager.GetLoginsAsync(user),
            BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
        };
        return View(model);
    }

    //
    // POST: /Manage/RemoveLogin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveLogin(string? loginProvider, string? providerKey)
    {
        ManageMessageId? message;
        var user = await CurrentUserAsync();
        var result = await _userManager.RemoveLoginAsync(user, loginProvider ?? string.Empty, providerKey ?? string.Empty);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            message = ManageMessageId.RemoveLoginSuccess;
        }
        else
        {
            message = ManageMessageId.Error;
        }
        return RedirectToAction("ManageLogins", new { Message = message });
    }

    //
    // GET: /Manage/AddPhoneNumber
    public IActionResult AddPhoneNumber()
    {
        return View();
    }

    //
    // POST: /Manage/AddPhoneNumber
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        // Generate the token and send it
        var user = await CurrentUserAsync();
        var code = await _userManager.GenerateChangePhoneNumberTokenAsync(user, model.Number!);
        await _smsSender.SendAsync(model.Number, "Your security code is: " + code);
        return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
    }

    //
    // POST: /Manage/EnableTwoFactorAuthentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EnableTwoFactorAuthentication()
    {
        var user = await CurrentUserAsync();
        await _userManager.SetTwoFactorEnabledAsync(user, true);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Manage");
    }

    //
    // POST: /Manage/DisableTwoFactorAuthentication
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DisableTwoFactorAuthentication()
    {
        var user = await CurrentUserAsync();
        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Manage");
    }

    //
    // GET: /Manage/VerifyPhoneNumber
    public IActionResult VerifyPhoneNumber(string? phoneNumber)
    {
        // Send an SMS through the SMS provider to verify the phone number
        return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
    }

    //
    // POST: /Manage/VerifyPhoneNumber
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await CurrentUserAsync();
        var result = await _userManager.ChangePhoneNumberAsync(user, model.PhoneNumber!, model.Code!);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
        }
        // If we got this far, something failed, redisplay form
        ModelState.AddModelError("", "Failed to verify phone");
        return View(model);
    }

    //
    // POST: /Manage/RemovePhoneNumber
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemovePhoneNumber()
    {
        var user = await CurrentUserAsync();
        var result = await _userManager.SetPhoneNumberAsync(user, null);
        if (!result.Succeeded)
        {
            return RedirectToAction("Index", new { Message = ManageMessageId.Error });
        }
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
    }

    //
    // GET: /Manage/ChangePassword
    public IActionResult ChangePassword()
    {
        return View();
    }

    //
    // POST: /Manage/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        var user = await CurrentUserAsync();
        var result = await _userManager.ChangePasswordAsync(user, model.OldPassword!, model.NewPassword!);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return RedirectToAction("Index", "Checklists");
        }
        AddErrors(result);
        return View(model);
    }

    //
    // GET: /Manage/SetPassword
    public IActionResult SetPassword()
    {
        return View();
    }

    //
    // POST: /Manage/SetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetPassword(SetPasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            var user = await CurrentUserAsync();
            var result = await _userManager.AddPasswordAsync(user, model.NewPassword!);
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
    // GET: /Manage/ManageLogins
    public async Task<IActionResult> ManageLogins(ManageMessageId? message)
    {
        ViewBag.StatusMessage =
            message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
            : message == ManageMessageId.Error ? "An error has occurred."
            : "";
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return View("Error");
        }
        var userLogins = await _userManager.GetLoginsAsync(user);
        var otherLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(auth => userLogins.All(ul => auth.Name != ul.LoginProvider))
            .ToList();
        ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
        return View(new ManageLoginsViewModel
        {
            CurrentLogins = userLogins,
            OtherLogins = otherLogins
        });
    }

    //
    // POST: /Manage/LinkLogin
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LinkLogin(string? provider)
    {
        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();
        if (!schemes.Any(s => s.Name == provider))
        {
            // OWIN answered a challenge for an unregistered provider with a 401, which the cookie middleware sent to the login page.
            return Challenge();
        }

        // Request a redirect to the external login provider to link a login for the current user
        var redirectUrl = Url.Action("LinkLoginCallback", "Manage");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, _userManager.GetUserId(User));
        return Challenge(properties, provider!);
    }

    //
    // GET: /Manage/LinkLoginCallback
    public async Task<IActionResult> LinkLoginCallback()
    {
        var user = await CurrentUserAsync();
        var loginInfo = await _signInManager.GetExternalLoginInfoAsync(user.Id);
        if (loginInfo == null)
        {
            return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }
        var result = await _userManager.AddLoginAsync(user, loginInfo);
        return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
    }

    #region Helpers

    private async Task<ApplicationUser> CurrentUserAsync() =>
        await _userManager.GetUserAsync(User)
        ?? throw new InvalidOperationException("The signed-in user no longer exists.");

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }
    }

    public enum ManageMessageId
    {
        AddPhoneSuccess,
        ChangePasswordSuccess,
        SetTwoFactorSuccess,
        SetPasswordSuccess,
        RemoveLoginSuccess,
        RemovePhoneSuccess,
        Error
    }

    #endregion
}
