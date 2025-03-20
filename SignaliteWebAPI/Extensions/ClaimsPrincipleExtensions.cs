using System.Security.Claims;
using SignaliteWebAPI.Infrastructure.Exceptions;

namespace SignaliteWebAPI.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            var userId = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                throw new TokenException("Cannot get id from token");
            }

            return userId;
        }

        public static string GetUsername(this ClaimsPrincipal user)
        {
            var username = user?.FindFirst(ClaimTypes.Name)?.Value;

            if (username == null)
            {
                throw new TokenException("Cannot get name from token");
            }

            return username;
        }
    }
}