using Hangfire.Dashboard;

namespace CateringEcommerce.API.Attributes
{
    /// <summary>
    /// Hangfire dashboard authorization filter
    /// In production, this should check if the user is authenticated and has admin role
    /// For development, it allows all access
    /// </summary>
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // For development: Allow all access
            // TODO: In production, check authentication and admin role
            // var httpContext = context.GetHttpContext();
            // return httpContext.User.Identity?.IsAuthenticated == true &&
            //        httpContext.User.IsInRole("Admin");

            return true; // Allow all for development
        }
    }
}
