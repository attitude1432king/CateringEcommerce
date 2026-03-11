using CateringEcommerce.Domain.Interfaces.Invoice;
using CateringEcommerce.Domain.Models.Invoice;
using CateringEcommerce.Domain.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CateringEcommerce.BAL.Services
{
    /// <summary>
    /// Invoice PDF generation service using QuestPDF
    /// Generates GST-compliant invoices with CGST/SGST breakdown
    /// </summary>
    public class InvoicePdfService : IInvoicePdfService
    {
        private readonly IInvoiceRepository _invoiceRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InvoicePdfService> _logger;
        private readonly string _logoPath;
        private readonly string _companyName;
        private readonly string _companyGstin;
        private readonly string _companyAddress;
        private readonly string _companyPhone;
        private readonly string _companyEmail;

        public InvoicePdfService(
            IInvoiceRepository invoiceRepository,
            IConfiguration configuration,
            ILogger<InvoicePdfService> logger)
        {
            _invoiceRepository = invoiceRepository;
            _configuration = configuration;
            _logger = logger;

            // Load company details from configuration
            _logoPath = configuration["Pdf:LogoPath"] ?? "wwwroot/logo.png";
            _companyName = configuration["Company:Name"] ?? "Catering Services Pvt. Ltd.";
            _companyGstin = configuration["Company:GSTIN"] ?? "27AABCU9603R1ZM";
            _companyAddress = configuration["Company:Address"] ?? "123 Business Park, Mumbai, Maharashtra 400001";
            _companyPhone = configuration["Company:Phone"] ?? "+91 22 1234 5678";
            _companyEmail = configuration["Company:Email"] ?? "invoices@cateringservices.com";

            // Configure QuestPDF license (Community license is free)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        #region Public Methods

        /// <summary>
        /// Generates PDF for an invoice
        /// Creates GST-compliant PDF with CGST/SGST breakdown
        /// </summary>
        public async Task<byte[]> GenerateInvoicePdfAsync(InvoiceDto invoice, bool includeLogo = true)
        {
            try
            {
                _logger.LogInformation("Generating PDF for Invoice {InvoiceId}", invoice.InvoiceId);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Calibri"));

                        page.Header().Element(c => ComposeHeader(c, invoice, includeLogo));
                        page.Content().Element(c => ComposeContent(c, invoice));
                        page.Footer().Element(c => ComposeFooter(c, invoice));
                    });
                });

                var pdfBytes = document.GeneratePdf();
                _logger.LogInformation("PDF generated successfully for Invoice {InvoiceId}, Size: {Size} KB",
                    invoice.InvoiceId, pdfBytes.Length / 1024);

                return pdfBytes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF for Invoice {InvoiceId}", invoice.InvoiceId);
                throw;
            }
        }

        /// <summary>
        /// Generates PDF and saves to file
        /// </summary>
        public async Task GenerateInvoicePdfToFileAsync(InvoiceDto invoice, string filePath, bool includeLogo = true)
        {
            var pdfBytes = await GenerateInvoicePdfAsync(invoice, includeLogo);
            await File.WriteAllBytesAsync(filePath, pdfBytes);
            _logger.LogInformation("PDF saved to {FilePath}", filePath);
        }

        /// <summary>
        /// Generates PDF for multiple invoices (bulk)
        /// </summary>
        public async Task<byte[]> GenerateBulkInvoicePdfAsync(List<InvoiceDto> invoices, bool includeLogo = true)
        {
            try
            {
                _logger.LogInformation("Generating bulk PDF for {Count} invoices", invoices.Count);

                var document = Document.Create(container =>
                {
                    foreach (var invoice in invoices)
                    {
                        container.Page(page =>
                        {
                            page.Size(PageSizes.A4);
                            page.Margin(40);
                            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Calibri"));

                            page.Header().Element(c => ComposeHeader(c, invoice, includeLogo));
                            page.Content().Element(c => ComposeContent(c, invoice));
                            page.Footer().Element(c => ComposeFooter(c, invoice));
                        });
                    }
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating bulk PDF");
                throw;
            }
        }

        /// <summary>
        /// Generates payment receipt PDF
        /// </summary>
        public async Task<byte[]> GeneratePaymentReceiptPdfAsync(InvoiceDto invoice, string paymentId)
        {
            try
            {
                _logger.LogInformation("Generating payment receipt for Invoice {InvoiceId}, Payment {PaymentId}",
                    invoice.InvoiceId, paymentId);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Calibri"));

                        page.Header().Element(c => ComposeReceiptHeader(c, invoice));
                        page.Content().Element(c => ComposeReceiptContent(c, invoice, paymentId));
                        page.Footer().Element(c => ComposeFooter(c, invoice));
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating payment receipt");
                throw;
            }
        }

        /// <summary>
        /// Generates credit note PDF (for refunds)
        /// </summary>
        public async Task<byte[]> GenerateCreditNotePdfAsync(InvoiceDto invoice, decimal refundAmount, string reason)
        {
            try
            {
                _logger.LogInformation("Generating credit note for Invoice {InvoiceId}, Amount: {Amount}",
                    invoice.InvoiceId, refundAmount);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Calibri"));

                        page.Header().Element(c => ComposeCreditNoteHeader(c, invoice));
                        page.Content().Element(c => ComposeCreditNoteContent(c, invoice, refundAmount, reason));
                        page.Footer().Element(c => ComposeFooter(c, invoice));
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating credit note");
                throw;
            }
        }

        /// <summary>
        /// Generates consolidated statement PDF
        /// </summary>
        public async Task<byte[]> GenerateConsolidatedStatementPdfAsync(long orderId)
        {
            try
            {
                _logger.LogInformation("Generating consolidated statement for Order {OrderId}", orderId);

                // Get all invoices for the order
                var invoices = await _invoiceRepository.GetInvoicesByOrderIdAsync(orderId);

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(40);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Calibri"));

                        page.Header().Element(c => ComposeConsolidatedHeader(c, orderId));
                        page.Content().Element(c => ComposeConsolidatedContent(c, invoices));
                        page.Footer().AlignCenter().Text(text =>
                        {
                            text.Span("Page ");
                            text.CurrentPageNumber();
                            text.Span(" of ");
                            text.TotalPages();
                        });
                    });
                });

                return document.GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating consolidated statement");
                throw;
            }
        }

        /// <summary>
        /// Validates PDF generation settings
        /// </summary>
        public async Task<PdfValidationResult> ValidatePdfSettingsAsync()
        {
            var result = new PdfValidationResult
            {
                IsValid = true,
                LogoPath = _logoPath
            };

            // Check logo file
            if (File.Exists(_logoPath))
            {
                result.LogoAvailable = true;
            }
            else
            {
                result.LogoAvailable = false;
                result.Warnings.Add($"Logo file not found at: {_logoPath}");
            }

            // Check company details
            if (string.IsNullOrWhiteSpace(_companyGstin))
            {
                result.Errors.Add("Company GSTIN not configured");
                result.IsValid = false;
            }

            if (string.IsNullOrWhiteSpace(_companyName))
            {
                result.Errors.Add("Company name not configured");
                result.IsValid = false;
            }

            result.TemplatesConfigured = result.IsValid;

            return await Task.FromResult(result);
        }

        #endregion

        #region Header Composition

        private void ComposeHeader(IContainer container, InvoiceDto invoice, bool includeLogo)
        {
            container.Column(column =>
            {
                // Logo and company info row
                column.Item().Row(row =>
                {
                    // Logo (left)
                    if (includeLogo && File.Exists(_logoPath))
                    {
                        row.ConstantItem(120).Image(_logoPath, ImageScaling.FitArea);
                    }

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().AlignRight().Text(_companyName).Bold().FontSize(16);
                        col.Item().AlignRight().Text(_companyAddress).FontSize(9);
                        col.Item().AlignRight().Text($"GSTIN: {_companyGstin}").FontSize(9).Bold();
                        col.Item().AlignRight().Text($"Phone: {_companyPhone}").FontSize(9);
                        col.Item().AlignRight().Text($"Email: {_companyEmail}").FontSize(9);
                    });
                });

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Darken2);

                // Invoice title
                column.Item().PaddingTop(15).AlignCenter().Text(text =>
                {
                    text.Span(invoice.IsProforma ? "PROFORMA INVOICE" : "TAX INVOICE")
                        .Bold()
                        .FontSize(18)
                        .FontColor(invoice.IsProforma ? Colors.Blue.Darken2 : Colors.Green.Darken2);
                });

                column.Item().PaddingTop(5).AlignCenter().Text($"Invoice Type: {GetInvoiceTypeName(invoice.InvoiceType)}")
                    .FontSize(11)
                    .Italic();

                column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Darken2);

                // Invoice details grid
                column.Item().PaddingTop(10).Row(row =>
                {
                    // Left column
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Invoice No: ").Bold();
                            text.Span(invoice.InvoiceNumber);
                        });
                        col.Item().Text(text =>
                        {
                            text.Span("Invoice Date: ").Bold();
                            text.Span(invoice.InvoiceDate.ToString("dd-MMM-yyyy"));
                        });
                        if (invoice.DueDate.HasValue)
                        {
                            col.Item().Text(text =>
                            {
                                text.Span("Due Date: ").Bold();
                                text.Span(invoice.DueDate.Value.ToString("dd-MMM-yyyy"))
                                    .FontColor(invoice.DueDate < DateTime.Now ? Colors.Red.Darken1 : Colors.Black);
                            });
                        }
                    });

                    // Right column
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text(text =>
                        {
                            text.Span("Order ID: ").Bold();
                            text.Span($"#{invoice.OrderId}");
                        });
                        if (invoice.EventId.HasValue)
                        {
                            col.Item().Text(text =>
                            {
                                text.Span("Event ID: ").Bold();
                                text.Span($"#{invoice.EventId}");
                            });
                        }
                        col.Item().Text(text =>
                        {
                            text.Span("Status: ").Bold();
                            text.Span(invoice.Status.ToString())
                                .FontColor(GetStatusColor(invoice.Status));
                        });
                    });
                });

                // Customer details
                column.Item().PaddingTop(15).Background(Colors.Grey.Lighten3).Padding(10).Column(col =>
                {
                    col.Item().Text("Bill To:").Bold().FontSize(11);
                    col.Item().PaddingTop(5).Text(invoice.OrderSummary?.CustomerName ?? "Customer");
                    if (!string.IsNullOrWhiteSpace(invoice.OrderSummary?.EventLocation))
                    {
                        col.Item().Text(invoice.OrderSummary?.EventLocation).FontSize(9);
                    }
                    if (!string.IsNullOrWhiteSpace(invoice.OrderSummary?.CustomerPhone))
                    {
                        col.Item().Text($"Phone: {invoice.OrderSummary?.CustomerPhone}").FontSize(9);
                    }
                    if (!string.IsNullOrWhiteSpace(invoice.CustomerGstin))
                    {
                        col.Item().Text($"GSTIN: {invoice.CustomerGstin}").FontSize(9).Bold();
                    }
                });
            });
        }

        private void ComposeReceiptHeader(IContainer container, InvoiceDto invoice)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("PAYMENT RECEIPT").Bold().FontSize(18).FontColor(Colors.Green.Darken2);
                column.Item().PaddingTop(5).AlignCenter().Text($"Invoice: {invoice.InvoiceNumber}").FontSize(11);
                column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
            });
        }

        private void ComposeCreditNoteHeader(IContainer container, InvoiceDto invoice)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("CREDIT NOTE").Bold().FontSize(18).FontColor(Colors.Red.Darken2);
                column.Item().PaddingTop(5).AlignCenter().Text($"Against Invoice: {invoice.InvoiceNumber}").FontSize(11);
                column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
            });
        }

        private void ComposeConsolidatedHeader(IContainer container, long orderId)
        {
            container.Column(column =>
            {
                column.Item().AlignCenter().Text("CONSOLIDATED STATEMENT").Bold().FontSize(18);
                column.Item().PaddingTop(5).AlignCenter().Text($"Order #{orderId}").FontSize(11);
                column.Item().PaddingTop(15).LineHorizontal(1).LineColor(Colors.Grey.Darken2);
            });
        }

        #endregion

        #region Content Composition

        private void ComposeContent(IContainer container, InvoiceDto invoice)
        {
            container.PaddingTop(15).Column(column =>
            {
                // Line items table
                column.Item().Table(table =>
                {
                    // Define columns
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(40); // S.No
                        columns.RelativeColumn(4);   // Description
                        columns.ConstantColumn(70);  // Quantity
                        columns.ConstantColumn(80);  // Rate
                        columns.ConstantColumn(80);  // Amount
                    });

                    // Header
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("S.No").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Description").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Quantity").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Rate (₹)").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Amount (₹)").FontColor(Colors.White).Bold();
                    });

                    // Line items
                    var lineNumber = 1;
                    foreach (var item in invoice.LineItems ?? new List<InvoiceLineItemDto>())
                    {
                        var bgColor = lineNumber % 2 == 0 ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().Background(bgColor).Padding(5).Text(lineNumber.ToString());
                        table.Cell().Background(bgColor).Padding(5).Text(item.Description);
                        table.Cell().Background(bgColor).Padding(5).Text($"{item.Quantity} {item.Unit}");
                        table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{item.UnitPrice:N2}");
                        table.Cell().Background(bgColor).Padding(5).AlignRight().Text($"{item.TotalAmount:N2}");

                        lineNumber++;
                    }
                });

                // Totals section
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem(); // Empty space

                    row.ConstantItem(250).Column(col =>
                    {
                        // Subtotal
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:").Bold();
                            r.ConstantItem(80).AlignRight().Text($"₹ {invoice.Subtotal:N2}");
                        });

                        // CGST
                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text($"CGST ({invoice.CgstPercent}%):");
                            r.ConstantItem(80).AlignRight().Text($"₹ {invoice.CgstAmount:N2}");
                        });

                        // SGST
                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text($"SGST ({invoice.SgstPercent}%):");
                            r.ConstantItem(80).AlignRight().Text($"₹ {invoice.SgstAmount:N2}");
                        });

                        col.Item().PaddingTop(5).LineHorizontal(1);

                        // Total
                        col.Item().PaddingTop(5).Background(Colors.Grey.Lighten3).Padding(5).Row(r =>
                        {
                            r.RelativeItem().Text("Total Amount:").Bold().FontSize(12);
                            r.ConstantItem(80).AlignRight().Text($"₹ {invoice.TotalAmount:N2}").Bold().FontSize(12);
                        });

                        // Amount paid
                        if (invoice.AmountPaid > 0)
                        {
                            col.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Amount Paid:").FontColor(Colors.Green.Darken1);
                                r.ConstantItem(80).AlignRight().Text($"₹ {invoice.AmountPaid:N2}").FontColor(Colors.Green.Darken1);
                            });
                        }

                        // Balance due
                        if (invoice.BalanceDue > 0)
                        {
                            col.Item().PaddingTop(5).Row(r =>
                            {
                                r.RelativeItem().Text("Balance Due:").Bold().FontColor(Colors.Red.Darken1);
                                r.ConstantItem(80).AlignRight().Text($"₹ {invoice.BalanceDue:N2}").Bold().FontColor(Colors.Red.Darken1);
                            });
                        }
                    });
                });

                // Amount in words
                column.Item().PaddingTop(15).Background(Colors.Blue.Lighten4).Padding(10).Text(text =>
                {
                    text.Span("Amount in Words: ").Bold();
                    text.Span(NumberToWords(invoice.TotalAmount) + " Rupees Only");
                });

                // Notes and terms
                if (!string.IsNullOrWhiteSpace(invoice.Notes))
                {
                    column.Item().PaddingTop(15).Column(col =>
                    {
                        col.Item().Text("Notes:").Bold().FontSize(11);
                        col.Item().PaddingTop(5).Text(invoice.Notes).FontSize(9);
                    });
                }

                // Payment terms
                column.Item().PaddingTop(15).Column(col =>
                {
                    col.Item().Text("Payment Terms:").Bold().FontSize(11);
                    col.Item().PaddingTop(5).Text(GetPaymentTerms(invoice.InvoiceType)).FontSize(9);
                });

                // SAC code for services
                column.Item().PaddingTop(10).Text("SAC Code: 996331 (Outdoor Catering Services)").FontSize(8).Italic();
            });
        }

        private void ComposeReceiptContent(IContainer container, InvoiceDto invoice, string paymentId)
        {
            container.PaddingTop(15).Column(column =>
            {
                column.Item().Background(Colors.Green.Lighten4).Padding(15).Column(col =>
                {
                    col.Item().Text("Payment Received Successfully").Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                    col.Item().PaddingTop(10).Row(r =>
                    {
                        r.RelativeItem().Text("Amount Paid:").Bold();
                        r.ConstantItem(120).AlignRight().Text($"₹ {invoice.AmountPaid:N2}").Bold().FontSize(12);
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Text("Payment ID:");
                        r.ConstantItem(120).AlignRight().Text(paymentId).FontSize(10);
                    });
                    col.Item().PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Text("Payment Date:");
                        r.ConstantItem(120).AlignRight().Text(DateTime.Now.ToString("dd-MMM-yyyy HH:mm"));
                    });
                });
            });
        }

        private void ComposeCreditNoteContent(IContainer container, InvoiceDto invoice, decimal refundAmount, string reason)
        {
            container.PaddingTop(15).Column(column =>
            {
                column.Item().Background(Colors.Red.Lighten4).Padding(15).Column(col =>
                {
                    col.Item().Text("Refund/Credit Note").Bold().FontSize(14);
                    col.Item().PaddingTop(10).Row(r =>
                    {
                        r.RelativeItem().Text("Refund Amount:").Bold();
                        r.ConstantItem(120).AlignRight().Text($"₹ {refundAmount:N2}").Bold().FontSize(12);
                    });
                    col.Item().PaddingTop(5).Text($"Reason: {reason}");
                });
            });
        }

        private void ComposeConsolidatedContent(IContainer container, List<InvoiceDto> invoices)
        {
            container.PaddingTop(15).Column(column =>
            {
                // Summary table
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(2);   // Invoice Number
                        columns.RelativeColumn(2);   // Type
                        columns.RelativeColumn(2);   // Date
                        columns.RelativeColumn(1);   // Amount
                        columns.RelativeColumn(1);   // Paid
                        columns.RelativeColumn(1);   // Balance
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Invoice No").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Type").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Date").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Amount").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Paid").FontColor(Colors.White).Bold();
                        header.Cell().Background(Colors.Grey.Darken2).Padding(5).Text("Balance").FontColor(Colors.White).Bold();
                    });

                    foreach (var invoice in invoices)
                    {
                        table.Cell().Padding(5).Text(invoice.InvoiceNumber);
                        table.Cell().Padding(5).Text(invoice.InvoiceType.ToString());
                        table.Cell().Padding(5).Text(invoice.InvoiceDate.ToString("dd-MMM-yyyy"));
                        table.Cell().Padding(5).AlignRight().Text($"₹{invoice.TotalAmount:N2}");
                        table.Cell().Padding(5).AlignRight().Text($"₹{invoice.AmountPaid:N2}");
                        table.Cell().Padding(5).AlignRight().Text($"₹{invoice.BalanceDue:N2}");
                    }
                });

                // Grand totals
                column.Item().PaddingTop(15).Row(row =>
                {
                    row.RelativeItem();
                    row.ConstantItem(250).Column(col =>
                    {
                        col.Item().Row(r =>
                        {
                            r.RelativeItem().Text("Total Amount:").Bold();
                            r.ConstantItem(80).AlignRight().Text($"₹ {invoices.Sum(i => i.TotalAmount):N2}").Bold();
                        });
                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("Total Paid:").FontColor(Colors.Green.Darken1);
                            r.ConstantItem(80).AlignRight().Text($"₹ {invoices.Sum(i => i.AmountPaid):N2}").FontColor(Colors.Green.Darken1);
                        });
                        col.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text("Total Balance:").Bold().FontColor(Colors.Red.Darken1);
                            r.ConstantItem(80).AlignRight().Text($"₹ {invoices.Sum(i => i.BalanceDue):N2}").Bold().FontColor(Colors.Red.Darken1);
                        });
                    });
                });
            });
        }

        #endregion

        #region Footer Composition

        private void ComposeFooter(IContainer container, InvoiceDto invoice)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(20).LineHorizontal(1).LineColor(Colors.Grey.Darken2);

                column.Item().PaddingTop(10).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("For " + _companyName).Bold();
                        col.Item().PaddingTop(30).Text("Authorized Signatory");
                    });

                    row.RelativeItem().AlignRight().Column(col =>
                    {
                        col.Item().Text("This is a computer generated invoice").FontSize(8).Italic();
                        col.Item().Text("No signature required").FontSize(8).Italic();
                    });
                });

                column.Item().PaddingTop(10).AlignCenter().Text(text =>
                {
                    text.Span("Page ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" of ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        }

        #endregion

        #region Helper Methods

        private string GetInvoiceTypeName(InvoiceType type)
        {
            return type switch
            {
                InvoiceType.BOOKING => "Booking Invoice (40% Advance)",
                InvoiceType.PRE_EVENT => "Pre-Event Invoice (35% Payment)",
                InvoiceType.FINAL => "Final Settlement Invoice (25% + Extras)",
                _ => type.ToString()
            };
        }

        private string GetPaymentTerms(InvoiceType type)
        {
            return type switch
            {
                InvoiceType.BOOKING => "Payment due within 3 days to confirm booking. Order will be cancelled if payment not received.",
                InvoiceType.PRE_EVENT => "Payment due 2 days before event. Event cannot proceed without this payment (75% total required).",
                InvoiceType.FINAL => "Final settlement due within 7 days after event completion. Includes any extra charges incurred.",
                _ => "Payment terms as per agreement."
            };
        }

        private string GetStatusColor(InvoiceStatus status)
        {
            return status switch
            {
                InvoiceStatus.PAID => "#22c55e", // Green
                InvoiceStatus.UNPAID => "#f59e0b", // Orange
                InvoiceStatus.OVERDUE => "#ef4444", // Red
                InvoiceStatus.CANCELLED => "#6b7280", // Gray
                InvoiceStatus.REFUNDED => "#3b82f6", // Blue
                _ => "#000000" // Black
            };
        }

        private string NumberToWords(decimal number)
        {
            // Simplified number to words conversion
            // For production, use a library like Humanizer
            int intPart = (int)number;

            if (intPart == 0) return "Zero";

            string[] ones = { "", "One", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten",
                            "Eleven", "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen", "Seventeen",
                            "Eighteen", "Nineteen" };
            string[] tens = { "", "", "Twenty", "Thirty", "Forty", "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };

            if (intPart < 20) return ones[intPart];
            if (intPart < 100) return tens[intPart / 10] + (intPart % 10 > 0 ? " " + ones[intPart % 10] : "");
            if (intPart < 1000) return ones[intPart / 100] + " Hundred" + (intPart % 100 > 0 ? " and " + NumberToWords(intPart % 100) : "");
            if (intPart < 100000) return NumberToWords(intPart / 1000) + " Thousand" + (intPart % 1000 > 0 ? " " + NumberToWords(intPart % 1000) : "");
            if (intPart < 10000000) return NumberToWords(intPart / 100000) + " Lakh" + (intPart % 100000 > 0 ? " " + NumberToWords(intPart % 100000) : "");

            return NumberToWords(intPart / 10000000) + " Crore" + (intPart % 10000000 > 0 ? " " + NumberToWords(intPart % 10000000) : "");
        }

        #endregion
    }
}
