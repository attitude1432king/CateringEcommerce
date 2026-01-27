namespace CateringEcommerce.Domain.Models.Admin
{
    #region Admin Login/Auth Models

    public class AdminLoginRequest
    {
        public string Username { get; set; } = string.Empty;
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
        public string RefreshToken { get; set; } = string.Empty;
    }

    #endregion
}
