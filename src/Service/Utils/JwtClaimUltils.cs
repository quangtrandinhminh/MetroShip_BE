using System.Security.Claims;
using AngleSharp.Attributes;
using MetroShip.Utility.Enums;
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

    public static IList<string> GetUserRole(IHttpContextAccessor accessor)
    {
        var userClaimsPrincipal = GetLoginedUser(accessor);
        return userClaimsPrincipal?.FindAll(ClaimTypes.Role).Select(x => x.Value).ToList()
               ?? new List<string>();
    }

    public static string GetUserId(IHttpContextAccessor accessor)
    {
        var userClaimsPrincipal = GetLoginedUser(accessor);
        return userClaimsPrincipal?.FindFirst(ClaimTypes.Sid)?.Value ?? string.Empty;
    }

    public static string? GetUserStation(IHttpContextAccessor accessor)
    {
        var user = GetLoginedUser(accessor);
        return user.FindFirst("StationId")?.Value;
    }

    public static AssignmentRoleEnum? GetAssignmentRole(IHttpContextAccessor accessor)
    {
        var user = GetLoginedUser(accessor);
        var roleClaim = user.FindFirst("AssignmentRole")?.Value;
        return Enum.TryParse<AssignmentRoleEnum>(roleClaim, out var role) ? role : null;
    }
}