using System.Security.Claims;
using SignaliteWebAPI.Application.Exceptions;

namespace SignaliteWebAPI.Extensions;

public static class ClaimsPrincipleExtensions
{
    public static int GetUserId(this ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null)
        {
            throw new AuthException("Cannot get id from token");
        }
        return int.Parse(userId);
    }
}