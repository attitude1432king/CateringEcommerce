using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CateringEcommerce.API.Filters
{
    public class OwnerAuthorizationFilter : IAuthorizationFilter
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

            if (string.IsNullOrEmpty(role) || !role.Equals("Owner", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new
                {
                    result = false,
                    message = "Access denied. Partner privileges required.",
                    type = "error"
                })
                {
                    StatusCode = 403
                };
                return;
            }

            var ownerId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerId) || !long.TryParse(ownerId, out var id) || id <= 0)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    result = false,
                    message = "Invalid partner session. Please login again.",
                    type = "error"
                });
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class OwnerAuthorizeAttribute : TypeFilterAttribute
    {
        public OwnerAuthorizeAttribute() : base(typeof(OwnerAuthorizationFilter))
        {
        }
    }
}
