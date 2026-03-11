using System;
using System.Collections.Generic;

namespace CateringEcommerce.Domain.Models.Owner
{
    // Create Ticket Request
    public class CreateSupportTicketDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Payment Issues, Orders & Bookings, Account & Settings, Technical Issue, Other
        public string Priority { get; set; } = "Medium"; // Low, Medium, High, Urgent
        public long? RelatedOrderId { get; set; }
    }

    // Ticket List Item
    public class SupportTicketItemDto
    {
        public long TicketId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long? RelatedOrderId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public int MessageCount { get; set; }
    }

    // Ticket Detail (with messages)
    public class SupportTicketDetailDto
    {
        public long TicketId { get; set; }
        public string TicketNumber { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public long? RelatedOrderId { get; set; }
        public string? ResolutionNotes { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public List<TicketMessageDto> Messages { get; set; } = new List<TicketMessageDto>();
    }

    // Ticket Message
    public class TicketMessageDto
    {
        public long MessageId { get; set; }
        public string SenderType { get; set; } = string.Empty; // Owner, Admin
        public string MessageText { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }

    // Send Message Request
    public class SendTicketMessageDto
    {
        public string MessageText { get; set; } = string.Empty;
    }

    // Filter DTO
    public class SupportTicketFilterDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; } // null = all, Open, InProgress, Resolved, Closed
        public string? Category { get; set; }
        public string? SortBy { get; set; } = "CreatedDate"; // CreatedDate, Priority
        public string? SortOrder { get; set; } = "DESC";
    }

    // Paginated Response
    public class PaginatedSupportTicketsDto
    {
        public List<SupportTicketItemDto> Tickets { get; set; } = new List<SupportTicketItemDto>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }

    // Ticket Stats
    public class SupportTicketStatsDto
    {
        public int TotalTickets { get; set; }
        public int OpenTickets { get; set; }
        public int InProgressTickets { get; set; }
        public int ResolvedTickets { get; set; }
        public int ClosedTickets { get; set; }
    }
}
