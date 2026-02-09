using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Admin
{
    #region Admin Login/Auth Models

    public class AdminLoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 128 characters")]
        public string Password { get; set; } = string.Empty;
    }

    public class AdminLoginResponse
    {
        public long AdminId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime LastLogin { get; set; }
        public string? ProfilePhoto { get; set; }
    }

    public class AdminModel
    {
        public long AdminId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Added for BCrypt authentication
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? ProfilePhoto { get; set; }
        public bool IsActive { get; set; }
        public int FailedLoginAttempts { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? LockedUntil { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLogin { get; set; }
    }

    public class AdminRefreshTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required")]
        [StringLength(500, ErrorMessage = "Invalid refresh token format")]
        public string RefreshToken { get; set; } = string.Empty;
    }

    #endregion
}
