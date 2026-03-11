using Hangfire.Dashboard;

namespace CateringEcommerce.API
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            // In production, add proper authentication
            // For now, only allow in development
            #if DEBUG
                return true;
            #else
                var httpContext = context.GetHttpContext();
                return httpContext.User.IsInRole("Admin") || httpContext.User.IsInRole("SuperAdmin");
            #endif
        }
    }
}
