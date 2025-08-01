using System.Security.Claims;

namespace WebApi.Extensions;

public static class AuthExtensions
{
    public static bool TryGetUserId(this HttpContext httpContext, out Guid userId) => httpContext.User.TryGetUserId(out userId);

    public static Guid? GetUserId(this HttpContext httpContext) => httpContext.User.GetUserId();

    public static Guid RequireUserId(this HttpContext httpContext) =>
        httpContext.GetUserId() ?? throw new UnauthorizedAccessException("User ID not found.");

    public static Guid? GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        if (claimsPrincipal.Identity is null || !claimsPrincipal.Identity.IsAuthenticated) return null;
        return Guid.TryParse(claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier), out Guid guid) ? guid : null;
    }

    public static bool TryGetUserId(this ClaimsPrincipal claimsPrincipal, out Guid userId)
    {
        userId = Guid.Empty;

        var res = claimsPrincipal.GetUserId();
        if (res is null) return false;

        userId = res.Value;
        return true;
    }
}
