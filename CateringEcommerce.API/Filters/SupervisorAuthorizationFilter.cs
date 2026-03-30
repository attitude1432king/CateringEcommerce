using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CateringEcommerce.API.Filters
{
    public class SupervisorAuthorizationFilter : IAuthorizationFilter
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
            if (string.IsNullOrEmpty(role) ||
                !role.Equals("Supervisor", StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new ObjectResult(new
                {
                    result = false,
                    message = "Access denied. Supervisor privileges required.",
                    type = "error"
                })
                {
                    StatusCode = 403
                };
                return;
            }

            var supervisorId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(supervisorId) || !long.TryParse(supervisorId, out var id) || id <= 0)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    result = false,
                    message = "Invalid supervisor session. Please login again.",
                    type = "error"
                });
                return;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class SupervisorAuthorizeAttribute : TypeFilterAttribute
    {
        public SupervisorAuthorizeAttribute() : base(typeof(SupervisorAuthorizationFilter))
        {
        }
    }
}
