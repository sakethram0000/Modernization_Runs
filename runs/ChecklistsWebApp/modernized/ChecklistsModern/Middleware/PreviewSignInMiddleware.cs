using ChecklistsModern.Models;
using ChecklistsModern.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ChecklistsModern.Middleware;

public class PreviewSignInMiddleware
{
    private readonly RequestDelegate _next;

    public PreviewSignInMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        UserManager<ApplicationUser> users,
        IUserClaimsPrincipalFactory<ApplicationUser> principalFactory,
        IOptions<PreviewOptions> preview)
    {
        var email = preview.Value.SignInAs;
        if (!string.IsNullOrWhiteSpace(email) && !context.Request.Path.StartsWithSegments("/Account/Login"))
        {
            var user = await users.FindByEmailAsync(email);
            if (user is not null)
            {
                context.User = await principalFactory.CreateAsync(user);
            }
        }

        await _next(context);
    }
}
