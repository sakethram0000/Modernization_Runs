using System.Security.Claims;
using System.Security.Principal;

namespace ChecklistsModern.Extensions;

public static class IdentityExtensions
{
    public static string GetFullName(this IPrincipal user)
    {
        if (user.Identity is not ClaimsIdentity identity)
        {
            return string.Empty;
        }

        return identity.FindFirst("FullName")?.Value ?? string.Empty;
    }
}
