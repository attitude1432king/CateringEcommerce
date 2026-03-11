using CateringEcommerce.Domain.Models.Invoice;

namespace CateringEcommerce.Domain.Interfaces.Invoice
{
    /// <summary>
    /// Invoice PDF generation service interface
    /// Generates GST-compliant PDF invoices with branding
    /// </summary>
    public interface IInvoicePdfService
    {
        /// <summary>
        /// Generates PDF for an invoice
        /// Creates GST-compliant PDF with CGST/SGST breakdown
        /// </summary>
        /// <param name="invoice">Invoice data</param>
        /// <param name="includeLogo">Include company logo in PDF</param>
        /// <returns>PDF byte array</returns>
        Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice, bool includeLogo = true);

        /// <summary>
        /// Generates PDF and saves to file
        /// </summary>
        /// <param name="invoice">Invoice data</param>
        /// <param name="filePath">Output file path</param>
        /// <param name="includeLogo">Include company logo in PDF</param>
        Task GenerateInvoicePdfToFileAsync(InvoiceDto invoice, string filePath, bool includeLogo = true);

        /// <summary>
        /// Generates PDF for multiple invoices (bulk)
        /// Creates single PDF with multiple invoices
        /// </summary>
        /// <param name="invoices">List of invoices</param>
        /// <param name="includeLogo">Include company logo in PDF</param>
        /// <returns>PDF byte array</returns>
        Task<byte[]> GenerateBulkInvoicePdfAsync(List<InvoiceDto> invoices, bool includeLogo = true);

        /// <summary>
        /// Generates payment receipt PDF
        /// Shows payment confirmation with transaction details
        /// </summary>
        /// <param name="invoice">Invoice with payment details</param>
        /// <param name="paymentId">Razorpay payment ID</param>
        /// <returns>PDF byte array</returns>
        Task<byte[]> GeneratePaymentReceiptPdfAsync(InvoiceDto invoice, string paymentId);

        /// <summary>
        /// Generates credit note PDF (for refunds)
        /// </summary>
        /// <param name="invoice">Original invoice</param>
        /// <param name="refundAmount">Refund amount</param>
        /// <param name="reason">Refund reason</param>
        /// <returns>PDF byte array</returns>
        Task<byte[]> GenerateCreditNotePdfAsync(InvoiceDto invoice, decimal refundAmount, string reason);

        /// <summary>
        /// Generates consolidated statement PDF
        /// Shows all invoices and payments for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>PDF byte array</returns>
        Task<byte[]> GenerateConsolidatedStatementPdfAsync(long orderId);

        /// <summary>
        /// Validates PDF generation settings
        /// Checks if logo exists, templates configured, etc.
        /// </summary>
        /// <returns>Validation result</returns>
        Task<PdfValidationResult> ValidatePdfSettingsAsync();
    }

    /// <summary>
    /// PDF validation result
    /// </summary>
    public class PdfValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public bool LogoAvailable { get; set; }
        public bool TemplatesConfigured { get; set; }
        public string LogoPath { get; set; }
    }
}
