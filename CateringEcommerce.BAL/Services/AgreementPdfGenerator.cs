using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.IO;

namespace CateringEcommerce.BAL.Services
{
    public class AgreementPdfGenerator
    {
        public static byte[] GenerateAgreementPdf(
        string agreementText,
        string businessName,
        string ownerName,
        string signatureBase64,
        DateTime acceptedDate)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                    // =========================
                    // HEADER (AUTO-SIZED)
                    // =========================
                    page.Header()
                        .Background(Colors.Grey.Lighten4)
                        .Padding(20)
                        .Column(column =>
                        {
                            column.Item().AlignCenter()
                                .Text("PARTNER AGREEMENT")
                                .FontSize(24)
                                .Bold()
                                .FontColor("#e11d48");

                            column.Item().AlignCenter().PaddingTop(5)
                                .Text("ENYVORA - Where Every Feast Begins")
                                .FontSize(14)
                                .FontColor(Colors.Grey.Darken2);

                            column.Item().AlignCenter().PaddingTop(3)
                                .Text($"Date: {acceptedDate:MMMM dd, yyyy}")
                                .FontSize(12)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    // =========================
                    // CONTENT
                    // =========================
                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            // Agreement text (auto paginated)
                            column.Item()
                                .PaddingBottom(20)
                                .Text(agreementText)
                                .FontSize(11)
                                .LineHeight(1.5f)
                                .Justify();

                            // Signature Section
                            column.Item().PaddingTop(30)
                                .Background(Colors.Grey.Lighten4)
                                .Border(2)
                                .BorderColor(Colors.Grey.Lighten2)
                                .Padding(20)
                                .Column(innerColumn =>
                                {
                                    innerColumn.Item()
                                        .Text("Partner's Digital Signature:")
                                        .Bold()
                                        .FontSize(12)
                                        .FontColor(Colors.Grey.Darken3);

                                    if (!string.IsNullOrWhiteSpace(signatureBase64))
                                    {
                                        try
                                        {
                                            string base64Data = signatureBase64.Contains(",")
                                                ? signatureBase64.Split(',')[1]
                                                : signatureBase64;

                                            byte[] signatureBytes = Convert.FromBase64String(base64Data);

                                            innerColumn.Item()
                                                .PaddingTop(10)
                                                .MaxWidth(300)
                                                .MaxHeight(120)
                                                .Image(signatureBytes)
                                                .FitArea();
                                        }
                                        catch
                                        {
                                            innerColumn.Item()
                                                .PaddingTop(10)
                                                .Text("[Signature could not be loaded]")
                                                .Italic()
                                                .FontColor(Colors.Red.Medium);
                                        }
                                    }
                                    else
                                    {
                                        innerColumn.Item()
                                            .PaddingTop(10)
                                            .Text("[No signature provided]")
                                            .Italic()
                                            .FontColor(Colors.Red.Medium);
                                    }

                                    innerColumn.Item().PaddingTop(15)
                                        .Column(detailsColumn =>
                                        {
                                            detailsColumn.Item().Row(row =>
                                            {
                                                row.RelativeItem().Text("Signed Date: ").Bold();
                                                row.RelativeItem().Text(acceptedDate.ToString("MMMM dd, yyyy"));
                                            });

                                            detailsColumn.Item().PaddingTop(3).Row(row =>
                                            {
                                                row.RelativeItem().Text("Business Name: ").Bold();
                                                row.RelativeItem().Text(businessName ?? "Not provided");
                                            });

                                            detailsColumn.Item().PaddingTop(3).Row(row =>
                                            {
                                                row.RelativeItem().Text("Owner Name: ").Bold();
                                                row.RelativeItem().Text(ownerName ?? "Not provided");
                                            });
                                        });
                                });
                        });

                    // =========================
                    // FOOTER (AUTO-SIZED)
                    // =========================
                    page.Footer()
                        .Padding(10)
                        .BorderTop(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Column(column =>
                        {
                            column.Item().AlignCenter()
                                .Text("This is a digitally signed agreement. For any queries, please contact ENYVORA support.")
                                .FontSize(9)
                                .FontColor(Colors.Grey.Darken1);

                            column.Item().AlignCenter().PaddingTop(5)
                                .Text($"© {DateTime.Now.Year} ENYVORA Platform. All rights reserved.")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Medium);
                        });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }


        public static string SaveAgreementPdf(
            byte[] pdfBytes,
            long ownerId,
            string baseUploadPath)
        {
            try
            {
                // Create directory structure: secure_uploads/owner_{id}/agreements/
                string ownerFolder = Path.Combine(baseUploadPath, "secure_uploads", $"owner{ownerId}", "Agreements");

                if (!Directory.Exists(ownerFolder))
                {
                    Directory.CreateDirectory(ownerFolder);
                }

                // Generate unique filename
                string fileName = $"agreement_{ownerId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                string filePath = Path.Combine(ownerFolder, fileName);

                // Save PDF file
                File.WriteAllBytes(filePath, pdfBytes);

                // Return relative path for database storage
                return Path.Combine("secure_uploads", $"owner{ownerId}", "Agreements", fileName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving agreement PDF: {ex.Message}", ex);
            }
        }
    }
}
