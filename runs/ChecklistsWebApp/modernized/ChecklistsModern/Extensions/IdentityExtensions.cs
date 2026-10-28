using System.Security.Claims;

namespace ChecklistsModern.Extensions;

public static class IdentityExtensions
{
    public static string GetFullName(this ClaimsPrincipal user)
    {
        var fullNameClaim = user.FindFirst("FullName");
        return fullNameClaim != null ? fullNameClaim.Value : string.Empty;
    }
}
