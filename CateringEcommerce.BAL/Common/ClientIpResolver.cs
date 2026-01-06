using System.Net;

namespace CateringEcommerce.BAL.Common
{
    public static class ClientIpResolver
    {
        public static string GetClientIp(HttpContext context)
        {
            string? ip =
                context.Request.Headers["CF-Connecting-IP"].FirstOrDefault() ??
                context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault() ??
                context.Connection.RemoteIpAddress?.ToString();

            // Normalize IPv6 localhost
            if (ip == "::1")
                return "127.0.0.1";

            return IPAddress.TryParse(ip, out _) ? ip! : "127.0.0.1";
        }
    }
}
