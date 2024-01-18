using System.Security.Claims;

namespace Wave.Utilities;

public static class ClaimsPrincipalUtilities {
    public static bool IsInRole(this ClaimsPrincipal principal, string roleName) {
        return principal.Claims.Any(
            c => c.Type == ClaimTypes.Role && 
                 string.Equals(c.Value, roleName,StringComparison.CurrentCultureIgnoreCase));
    }
}