namespace CateringEcommerce.Domain.Models.Admin
{
    public class ErrorLogEntry
    {
        public long Id { get; set; }
        public Guid ErrorId { get; set; }
        public string? Message { get; set; }
        public string? ExceptionType { get; set; }
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
        public string? Source { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public string? QueryParams { get; set; }
        public string? RequestBody { get; set; }
        public int ResponseStatusCode { get; set; }
        public long? UserId { get; set; }
        public string UserRole { get; set; } = "Anonymous";
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? TraceId { get; set; }
        public string? CorrelationId { get; set; }
        public string? Environment { get; set; }
        public string? MachineName { get; set; }
        public string? ApplicationName { get; set; }
        public string LogLevel { get; set; } = "Error";
        public int? ExecutionTimeMs { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ErrorLogListRequest
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? ErrorId { get; set; }
        public long? UserId { get; set; }
        public string? UserRole { get; set; }
        public string? RequestPath { get; set; }
        public string? HttpMethod { get; set; }
        public int? StatusCode { get; set; }
        public string? Environment { get; set; }
        public string? Keyword { get; set; }
        public string? SortBy { get; set; } = "CreatedAt";
        public string? SortOrder { get; set; } = "DESC";
    }

    public class ErrorLogListResponse
    {
        public List<ErrorLogListItem> Logs { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    public class ErrorLogListItem
    {
        public long Id { get; set; }
        public Guid ErrorId { get; set; }
        public string? Message { get; set; }
        public string? ExceptionType { get; set; }
        public string? Source { get; set; }
        public string? RequestPath { get; set; }
        public string? RequestMethod { get; set; }
        public int ResponseStatusCode { get; set; }
        public long? UserId { get; set; }
        public string UserRole { get; set; } = "Anonymous";
        public string? Environment { get; set; }
        public string LogLevel { get; set; } = "Error";
        public DateTime CreatedAt { get; set; }
    }

    public class ErrorLogDetail : ErrorLogListItem
    {
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
        public string? QueryParams { get; set; }
        public string? RequestBody { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public string? TraceId { get; set; }
        public string? CorrelationId { get; set; }
        public string? MachineName { get; set; }
        public string? ApplicationName { get; set; }
        public int? ExecutionTimeMs { get; set; }
    }
}
