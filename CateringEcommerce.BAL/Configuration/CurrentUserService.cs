using CateringEcommerce.Domain.Interfaces.Common;
using System.Security.Claims;

namespace CateringEcommerce.BAL.Configuration
{
    public class CurrentUserService : ICurrentUserService
    {
        public Int64 UserId { get; }
        public string PhoneNumber { get; }
        public string UserRole { get; }

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            var user = httpContextAccessor.HttpContext?.User;

            if (user?.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = user.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier && !string.IsNullOrWhiteSpace(c.Value)).FirstOrDefault();
                var phoneClaim = user.FindFirst(ClaimTypes.MobilePhone);
                var roleClaim = user.FindFirst(ClaimTypes.Role);    

                if (long.TryParse(userIdClaim?.Value, out var id))
                    UserId = id;

                PhoneNumber = phoneClaim?.Value;
                UserRole = roleClaim?.Value;
            }
        }
    }

    public class EncryptionSettings
    {
        public string CustomKey { get; set; }
    }

}
