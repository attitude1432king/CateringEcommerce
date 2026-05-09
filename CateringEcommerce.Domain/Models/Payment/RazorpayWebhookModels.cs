namespace CateringEcommerce.Domain.Models.Payment
{
    public class RazorpayWebhookRequest
    {
        public string RawBody { get; set; } = string.Empty;
        public string? Signature { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }

    public class RazorpayWebhookProcessingResult
    {
        public int StatusCode { get; set; } = 200;
        public string Status { get; set; } = "processed";
        public string Message { get; set; } = string.Empty;
        public bool IsValid { get; set; }
        public long? WebhookLogId { get; set; }

        public static RazorpayWebhookProcessingResult Invalid(int statusCode, string message, long? logId = null)
        {
            return new RazorpayWebhookProcessingResult
            {
                StatusCode = statusCode,
                Status = "invalid",
                Message = message,
                IsValid = false,
                WebhookLogId = logId
            };
        }

        public static RazorpayWebhookProcessingResult Success(string status, string message, long? logId = null)
        {
            return new RazorpayWebhookProcessingResult
            {
                StatusCode = 200,
                Status = status,
                Message = message,
                IsValid = true,
                WebhookLogId = logId
            };
        }
    }

    public class RazorpayWebhookLogEntry
    {
        public long Id { get; set; }
        public string? EventType { get; set; }
        public string? PaymentId { get; set; }
        public string? OrderId { get; set; }
        public string Payload { get; set; } = "{}";
        public string? Signature { get; set; }
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public string ProcessingStatus { get; set; } = "received";
    }

    public class RazorpayPaymentTransactionUpsert
    {
        public string PaymentId { get; set; } = string.Empty;
        public string? RazorpayOrderId { get; set; }
        public long OrderId { get; set; }
        public string? StageType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string Status { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string? PaymentMethod { get; set; }
        public string? Signature { get; set; }
        public long WebhookLogId { get; set; }
        public string Payload { get; set; } = "{}";
    }

    public class RazorpayWebhookPaymentData
    {
        public string EventType { get; set; } = string.Empty;
        public string? PaymentId { get; set; }
        public string? RazorpayOrderId { get; set; }
        public long? ApplicationOrderId { get; set; }
        public string? StageType { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "INR";
        public string? PaymentStatus { get; set; }
        public string? PaymentMethod { get; set; }
    }
}
