using ChecklistsModern.Models;
using Microsoft.AspNetCore.Identity;

namespace ChecklistsModern.Identity;

// Identity 2 returned every broken password rule as one error, joined with a space.
public class LegacyPasswordValidator : PasswordValidator<ApplicationUser>
{
    public LegacyPasswordValidator(IdentityErrorDescriber describer)
        : base(describer)
    {
    }

    public override async Task<IdentityResult> ValidateAsync(UserManager<ApplicationUser> manager, ApplicationUser user, string? password)
    {
        var result = await base.ValidateAsync(manager, user, password);
        if (result.Succeeded)
            return result;

        return IdentityResult.Failed(new IdentityError
        {
            Code = "PasswordRules",
            Description = string.Join(" ", result.Errors.Select(e => e.Description))
        });
    }
}
