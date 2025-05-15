using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MetroShip.Service.Utils;

public class JwtClaimUltils
{
    public static ClaimsPrincipal? GetLoginedUser(IHttpContextAccessor accessor)
    {
        return accessor?.HttpContext?.User;
    }

    public static int GetUserId(ClaimsPrincipal userClaimsPrincipal)
    {
        return int.Parse(userClaimsPrincipal.FindFirst(ClaimTypes.Sid)?.Value);
    }

    public static IList<string> GetUserRole(ClaimsPrincipal userClaimsPrincipal)
    {
        return userClaimsPrincipal.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList();
    }
}