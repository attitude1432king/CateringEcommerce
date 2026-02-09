using System.ComponentModel.DataAnnotations;

namespace CateringEcommerce.Domain.Models.Owner
{
    /// <summary>
    /// Strongly-typed filter model for staff queries
    /// Replaces unsafe JSON string parameters to prevent SQL injection
    /// </summary>
    public class StaffFilterRequest
    {
        /// <summary>
        /// Filter by staff name (partial match)
        /// </summary>
        [StringLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
        public string? Name { get; set; }

        /// <summary>
        /// Filter by staff contact number
        /// </summary>
        [StringLength(20, ErrorMessage = "Contact cannot exceed 20 characters")]
        [RegularExpression(@"^[\d\+\-\(\)\s]*$", ErrorMessage = "Contact can only contain digits, +, -, (, ), and spaces")]
        public string? Contact { get; set; }

        /// <summary>
        /// Filter by staff role
        /// </summary>
        [StringLength(100, ErrorMessage = "Role cannot exceed 100 characters")]
        public string? Role { get; set; }

        /// <summary>
        /// Filter by status (0 = Inactive, 1 = Active, etc.)
        /// </summary>
        [Range(0, 100, ErrorMessage = "Status must be between 0 and 100")]
        public int? Status { get; set; }

        /// <summary>
        /// Filter by employment type
        /// </summary>
        [StringLength(50, ErrorMessage = "Employment type cannot exceed 50 characters")]
        public string? EmploymentType { get; set; }

        /// <summary>
        /// Filter by department
        /// </summary>
        [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
        public string? Department { get; set; }

        /// <summary>
        /// Filter staff created after this date
        /// </summary>
        public DateTime? CreatedAfter { get; set; }

        /// <summary>
        /// Filter staff created before this date
        /// </summary>
        public DateTime? CreatedBefore { get; set; }

        /// <summary>
        /// Page number for pagination
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be at least 1")]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Number of items per page
        /// </summary>
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// Sort by field name
        /// </summary>
        [StringLength(50, ErrorMessage = "Sort field cannot exceed 50 characters")]
        [RegularExpression(@"^[a-zA-Z_]+$", ErrorMessage = "Sort field can only contain letters and underscores")]
        public string? SortBy { get; set; }

        /// <summary>
        /// Sort direction (asc or desc)
        /// </summary>
        [RegularExpression(@"^(asc|desc|ASC|DESC)$", ErrorMessage = "Sort direction must be 'asc' or 'desc'")]
        public string SortDirection { get; set; } = "asc";
    }

    /// <summary>
    /// Response model for staff count
    /// </summary>
    public class StaffCountResponse
    {
        public int TotalCount { get; set; }
        public int ActiveCount { get; set; }
        public int InactiveCount { get; set; }
        public Dictionary<string, int>? CountByRole { get; set; }
    }

    /// <summary>
    /// Response model for staff list with pagination
    /// </summary>
    public class StaffListResponse
    {
        public List<StaffDto>? Items { get; set; }
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}
