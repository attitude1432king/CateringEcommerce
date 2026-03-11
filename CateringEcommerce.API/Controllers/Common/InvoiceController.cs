using CateringEcommerce.Domain.Interfaces.Invoice;
using CateringEcommerce.Domain.Models.Invoice;
using CateringEcommerce.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CateringEcommerce.API.Controllers.Common
{
    /// <summary>
    /// Invoice management endpoints
    /// Handles invoice generation, PDF download, and payment linking
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IInvoicePdfService _pdfService;
        private readonly ILogger<InvoiceController> _logger;

        public InvoiceController(
            IInvoiceRepository invoiceRepository,
            IInvoicePdfService pdfService,
            ILogger<InvoiceController> logger)
        {
            _invoiceRepository = invoiceRepository;
            _pdfService = pdfService;
            _logger = logger;
        }

        #region Invoice Retrieval

        /// <summary>
        /// Get all invoices for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>List of invoices</returns>
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetInvoicesByOrder(long orderId)
        {
            try
            {
                var invoices = await _invoiceRepository.GetInvoicesByOrderIdAsync(orderId);

                return Ok(new
                {
                    success = true,
                    data = invoices,
                    count = invoices.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices for order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Failed to retrieve invoices" });
            }
        }

        /// <summary>
        /// Get invoice by ID
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <returns>Invoice details</returns>
        [HttpGet("{invoiceId}")]
        [Authorize]
        public async Task<IActionResult> GetInvoiceById(long invoiceId)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

                if (invoice == null)
                    return NotFound(new { success = false, message = "Invoice not found" });

                return Ok(new
                {
                    success = true,
                    data = invoice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new { success = false, message = "Failed to retrieve invoice" });
            }
        }

        /// <summary>
        /// Get invoice by invoice number
        /// </summary>
        /// <param name="invoiceNumber">Invoice number</param>
        /// <returns>Invoice details</returns>
        [HttpGet("number/{invoiceNumber}")]
        [Authorize]
        public async Task<IActionResult> GetInvoiceByNumber(string invoiceNumber)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByNumberAsync(invoiceNumber);

                if (invoice == null)
                    return NotFound(new { success = false, message = "Invoice not found" });

                return Ok(new
                {
                    success = true,
                    data = invoice
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice {InvoiceNumber}", invoiceNumber);
                return StatusCode(500, new { success = false, message = "Failed to retrieve invoice" });
            }
        }

        /// <summary>
        /// Get invoices for a user (paginated)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Page size (default: 10)</param>
        /// <param name="status">Filter by status (optional)</param>
        /// <param name="type">Filter by type (optional)</param>
        /// <returns>Paginated invoice list</returns>
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<IActionResult> GetUserInvoices(
            long userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] InvoiceStatus? status = null,
            [FromQuery] InvoiceType? type = null)
        {
            try
            {
                var result = await _invoiceRepository.GetInvoicesByUserAsync(
                    userId,
                    pageNumber,
                    pageSize,
                    status,
                    type);

                return Ok(new
                {
                    success = true,
                    data = result.Invoices,
                    totalRecords = result.TotalCount,
                    currentPage = pageNumber,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)result.TotalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoices for user {UserId}", userId);
                return StatusCode(500, new { success = false, message = "Failed to retrieve invoices" });
            }
        }

        #endregion

        #region Invoice Generation

        /// <summary>
        /// Generate new invoice for an order
        /// Admin/Owner only
        /// </summary>
        /// <param name="dto">Invoice generation data</param>
        /// <returns>Created invoice ID</returns>
        [HttpPost("generate")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> GenerateInvoice([FromBody] InvoiceGenerationRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, errors = ModelState });

                var invoiceId = await _invoiceRepository.GenerateInvoiceAsync(dto);

                if (invoiceId <= 0)
                    return BadRequest(new { success = false, message = "Failed to generate invoice" });

                // Get the created invoice
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

                _logger.LogInformation(
                    "Invoice {InvoiceId} generated for Order {OrderId}, Type: {Type}",
                    invoiceId,
                    dto.OrderId,
                    dto.InvoiceType);

                return CreatedAtAction(
                    nameof(GetInvoiceById),
                    new { invoiceId },
                    new
                    {
                        success = true,
                        message = "Invoice generated successfully",
                        data = invoice
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating invoice for order {OrderId}", dto.OrderId);
                return StatusCode(500, new { success = false, message = "Failed to generate invoice" });
            }
        }

        #endregion

        #region PDF Operations

        /// <summary>
        /// Download invoice as PDF
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="includeLogo">Include company logo (default: true)</param>
        /// <returns>PDF file</returns>
        [HttpGet("{invoiceId}/download")]
        [Authorize]
        public async Task<IActionResult> DownloadInvoice(long invoiceId, [FromQuery] bool includeLogo = true)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

                if (invoice == null)
                    return NotFound(new { success = false, message = "Invoice not found" });

                // Generate PDF
                var pdfBytes = await _pdfService.GenerateInvoicePdfAsync(invoice, includeLogo);

                var fileName = $"Invoice_{invoice.InvoiceNumber}.pdf";

                _logger.LogInformation("Invoice {InvoiceId} downloaded", invoiceId);

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new { success = false, message = "Failed to download invoice" });
            }
        }

        /// <summary>
        /// Download payment receipt as PDF
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="paymentId">Razorpay payment ID</param>
        /// <returns>PDF file</returns>
        [HttpGet("{invoiceId}/receipt")]
        [Authorize]
        public async Task<IActionResult> DownloadReceipt(long invoiceId, [FromQuery] string paymentId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentId))
                    return BadRequest(new { success = false, message = "Payment ID required" });

                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

                if (invoice == null)
                    return NotFound(new { success = false, message = "Invoice not found" });

                if (invoice.Status != InvoiceStatus.PAID && invoice.Status != InvoiceStatus.PARTIALLY_PAID)
                    return BadRequest(new { success = false, message = "Invoice not paid" });

                // Generate receipt PDF
                var pdfBytes = await _pdfService.GeneratePaymentReceiptPdfAsync(invoice, paymentId);

                var fileName = $"Receipt_{invoice.InvoiceNumber}_{paymentId}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading receipt for invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new { success = false, message = "Failed to download receipt" });
            }
        }

        /// <summary>
        /// Download consolidated statement for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>PDF file</returns>
        [HttpGet("order/{orderId}/statement")]
        [Authorize]
        public async Task<IActionResult> DownloadConsolidatedStatement(long orderId)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateConsolidatedStatementPdfAsync(orderId);

                var fileName = $"Statement_Order_{orderId}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading statement for order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Failed to download statement" });
            }
        }

        /// <summary>
        /// Generate credit note PDF
        /// Admin/Owner only
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="dto">Credit note data</param>
        /// <returns>PDF file</returns>
        [HttpPost("{invoiceId}/credit-note")]
        [Authorize(Roles = "Admin,Owner")]
        public async Task<IActionResult> GenerateCreditNote(
            long invoiceId,
            [FromBody] CreditNoteDto dto)
        {
            try
            {
                var invoice = await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

                if (invoice == null)
                    return NotFound(new { success = false, message = "Invoice not found" });

                var pdfBytes = await _pdfService.GenerateCreditNotePdfAsync(
                    invoice,
                    dto.RefundAmount,
                    dto.Reason);

                var fileName = $"CreditNote_{invoice.InvoiceNumber}.pdf";

                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating credit note for invoice {InvoiceId}", invoiceId);
                return StatusCode(500, new { success = false, message = "Failed to generate credit note" });
            }
        }

        #endregion

        #region Invoice Updates

        /// <summary>
        /// Update invoice status
        /// Admin/System only
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="status">New status</param>
        /// <returns>Success result</returns>
        [HttpPut("{invoiceId}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateInvoiceStatus(
            long invoiceId,
            [FromBody] InvoiceStatus status)
        {
            try
            {
                var success = await _invoiceRepository.UpdateInvoiceStatusAsync(invoiceId, status);

                if (!success)
                    return BadRequest(new { success = false, message = "Failed to update invoice status" });

                _logger.LogInformation("Invoice {InvoiceId} status updated to {Status}", invoiceId, status);

                return Ok(new { success = true, message = "Invoice status updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating invoice {InvoiceId} status", invoiceId);
                return StatusCode(500, new { success = false, message = "Failed to update invoice status" });
            }
        }

        /// <summary>
        /// Link payment to invoice
        /// Called after Razorpay payment verification
        /// </summary>
        /// <param name="dto">Payment linkage data</param>
        /// <returns>Success result</returns>
        [HttpPost("link-payment")]
        [Authorize]
        public async Task<IActionResult> LinkPaymentToInvoice([FromBody] LinkPaymentToInvoiceDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, errors = ModelState });

                var success = await _invoiceRepository.LinkPaymentToInvoiceAsync(dto);

                if (!success)
                    return BadRequest(new { success = false, message = "Failed to link payment" });

                _logger.LogInformation(
                    "Payment {PaymentId} linked to Invoice {InvoiceId}, Amount: {Amount}",
                    dto.RazorpayPaymentId,
                    dto.InvoiceId,
                    dto.AmountPaid);

                return Ok(new
                {
                    success = true,
                    message = "Payment linked successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error linking payment to invoice {InvoiceId}", dto.InvoiceId);
                return StatusCode(500, new { success = false, message = "Failed to link payment" });
            }
        }

        #endregion

        #region Statistics and Reports

        /// <summary>
        /// Get invoice statistics for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Invoice statistics</returns>
        [HttpGet("order/{orderId}/statistics")]
        [Authorize]
        public async Task<IActionResult> GetOrderInvoiceStatistics(long orderId)
        {
            try
            {
                var stats = await _invoiceRepository.GetOrderInvoiceStatsAsync(orderId);

                if (stats == null)
                    return NotFound(new { success = false, message = "Order not found" });

                return Ok(new
                {
                    success = true,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice statistics for order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Failed to retrieve statistics" });
            }
        }

        /// <summary>
        /// Get total paid amount for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Total paid amount</returns>
        [HttpGet("order/{orderId}/total-paid")]
        [Authorize]
        public async Task<IActionResult> GetTotalPaidAmount(long orderId)
        {
            try
            {
                var totalPaid = await _invoiceRepository.GetTotalPaidAmountAsync(orderId);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orderId,
                        totalPaid,
                        formattedAmount = $"₹{totalPaid:N2}"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting total paid for order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Failed to retrieve total paid" });
            }
        }

        /// <summary>
        /// Get payment progress percentage for an order
        /// </summary>
        /// <param name="orderId">Order ID</param>
        /// <returns>Payment progress percentage (0-100)</returns>
        [HttpGet("order/{orderId}/progress")]
        [Authorize]
        public async Task<IActionResult> GetPaymentProgress(long orderId)
        {
            try
            {
                var percentage = await _invoiceRepository.GetPaymentProgressPercentageAsync(orderId);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        orderId,
                        progressPercentage = percentage,
                        formattedProgress = $"{percentage:N1}%",
                        milestones = new
                        {
                            booking = new { required = 40m, completed = percentage >= 40m },
                            preEvent = new { required = 75m, completed = percentage >= 75m },
                            fullPayment = new { required = 100m, completed = percentage >= 100m }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment progress for order {OrderId}", orderId);
                return StatusCode(500, new { success = false, message = "Failed to retrieve payment progress" });
            }
        }

        #endregion

        #region Validation

        /// <summary>
        /// Validate PDF settings
        /// Admin only - for troubleshooting
        /// </summary>
        /// <returns>Validation result</returns>
        [HttpGet("pdf/validate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ValidatePdfSettings()
        {
            try
            {
                var result = await _pdfService.ValidatePdfSettingsAsync();

                return Ok(new
                {
                    success = result.IsValid,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating PDF settings");
                return StatusCode(500, new { success = false, message = "Validation failed" });
            }
        }

        #endregion
    }

    /// <summary>
    /// DTO for credit note generation
    /// </summary>
    public class CreditNoteDto
    {
        public decimal RefundAmount { get; set; }
        public string Reason { get; set; }
    }
}
