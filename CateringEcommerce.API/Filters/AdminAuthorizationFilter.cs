using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CateringEcommerce.API.Filters
{
    /// <summary>
    /// Authorization filter to ensure only Admin or SuperAdmin roles can access admin endpoints
    /// </summary>
    public class AdminAuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Check if user is authenticated
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

            // Check if user has Admin or SuperAdmin role
            var role = user.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(role) ||
                (!role.Equals("System Admin", StringComparison.OrdinalIgnoreCase) &&
                 !role.Equals("Super Admin", StringComparison.OrdinalIgnoreCase)))
            {
                context.Result = new ForbidResult();
                context.HttpContext.Response.StatusCode = 403;
                context.Result = new ObjectResult(new
                {
                    result = false,
                    message = "Access denied. Admin privileges required.",
                    type = "error"
                })
                {
                    StatusCode = 403
                };
            }
        }
    }

    /// <summary>
    /// Attribute to apply Admin authorization filter to controllers or actions
    /// Usage: [AdminAuthorize]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class AdminAuthorizeAttribute : TypeFilterAttribute
    {
        public AdminAuthorizeAttribute() : base(typeof(AdminAuthorizationFilter))
        {
        }
    }
}
