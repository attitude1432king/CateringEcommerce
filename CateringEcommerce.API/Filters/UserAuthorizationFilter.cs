using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CateringEcommerce.API.Filters
{
    public class UserAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    result = false,
                    message = "Authentication required. Please login.",
                    type = "error"
                });
                return;
            }

            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role) || !role.Equals("User", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new
                {
                    result = false,
                    message = "Access denied. User privileges required.",
                    type = "error"
                })
                {
                    StatusCode = 403
                };
                return;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId) || !long.TryParse(userId, out var id) || id <= 0)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    result = false,
                    message = "Invalid user session. Please login again.",
                    type = "error"
                });
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class UserAuthorizeAttribute : TypeFilterAttribute
    {
        public UserAuthorizeAttribute() : base(typeof(UserAuthorizationFilter))
        {
        }
    }
}
