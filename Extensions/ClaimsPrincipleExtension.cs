using System.Security.Claims;

namespace API.Extensions
{
    public static class ClaimsPrincipleExtension
    {
        public static string GetUsername(this ClaimsPrincipal user)
        {
            var username = (user.FindFirst(ClaimTypes.Name)?.Value) ?? throw new Exception("Cannot get username from token");
            return username;
        }  
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var userId = int.Parse((user.FindFirst(ClaimTypes.NameIdentifier)?.Value) ?? throw new Exception("Cannot get id from token"));
            return userId;
        }
    }
}