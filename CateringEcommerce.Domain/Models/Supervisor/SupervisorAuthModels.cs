namespace CateringEcommerce.Domain.Models.Supervisor
{
    public class SupervisorLoginRequest
    {
        public string Identifier { get; set; } = string.Empty; // email or phone
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Lightweight model returned only for login credential verification.
    /// Never exposed outside the auth flow.
    /// </summary>
    public class SupervisorLoginInfo
    {
        public long SupervisorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string CurrentStatus { get; set; } = string.Empty;
        public string SupervisorType { get; set; } = string.Empty;
        public string AuthorityLevel { get; set; } = string.Empty;
    }
}
