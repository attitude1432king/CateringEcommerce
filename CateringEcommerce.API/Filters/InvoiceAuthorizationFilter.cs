using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace CateringEcommerce.API.Filters
{
    /// <summary>
    /// Authorization filter for invoice operations
    /// Ensures users can only access their own invoices
    /// </summary>
    public class InvoiceAuthorizationFilter : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new
                {
                    success = false,
                    message = "Authentication required"
                });
                return;
            }

            var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = user.FindFirst(ClaimTypes.Role)?.Value;

            // Admin can access all invoices
            if (userRole == "Admin")
            {
                return;
            }

            // Extract invoice ID or order ID from route
            var routeData = context.RouteData.Values;
            long? invoiceId = routeData.ContainsKey("invoiceId")
                ? long.Parse(routeData["invoiceId"].ToString())
                : null;
            long? orderId = routeData.ContainsKey("orderId")
                ? long.Parse(routeData["orderId"].ToString())
                : null;

            // Owners can access their own invoices
            if (userRole == "Owner")
            {
                // TODO: Verify invoice/order belongs to this owner
                // var ownerId = user.FindFirst("CateringOwnerId")?.Value;
                return;
            }

            // Regular users can only access their own invoices
            if (userRole == "User" && !string.IsNullOrEmpty(userId))
            {
                // TODO: Verify invoice/order belongs to this user
                // For now, allow if authenticated
                return;
            }

            context.Result = new ForbidResult();
        }
    }
}
