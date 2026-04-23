using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Invoice;
using CateringEcommerce.Domain.Models.Invoice;
using System.Data;
using Npgsql;
using NpgsqlTypes;

namespace CateringEcommerce.BAL.Base.Common
{
    /// <summary>
    /// Invoice repository implementation
    /// Handles all database operations for invoices, payment schedules, and audit logs
    /// Uses stored procedures for complex operations and direct queries for simple ones
    /// </summary>
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public InvoiceRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
        }

        #region Invoice Generation

        /// <summary>
        /// Generates a new invoice based on order and invoice type
        /// Auto-calculates amounts, GST, and creates line items
        /// Returns the generated invoice ID
        /// </summary>
        public async Task<long> GenerateInvoiceAsync(InvoiceGenerationRequestDto request)
        {
            try
            {
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", request.OrderId),
                    new NpgsqlParameter("@InvoiceType", (int)request.InvoiceType),
                    new NpgsqlParameter("@TriggeredBy", (object)request.TriggeredBy ?? DBNull.Value),
                    new NpgsqlParameter("@TriggeredByType", request.TriggeredByType?.ToString() ?? "SYSTEM"),
                    new NpgsqlParameter("@ExtraGuestCount", request.ExtraGuestCount ?? 0),
                    new NpgsqlParameter("@ExtraGuestCharges", request.ExtraGuestCharges ?? 0),
                    new NpgsqlParameter("@AddonCharges", request.AddonCharges ?? 0),
                    new NpgsqlParameter("@OvertimeCharges", request.OvertimeCharges ?? 0),
                    new NpgsqlParameter("@OtherCharges", request.OtherCharges ?? 0),
                    new NpgsqlParameter("@InvoiceId", NpgsqlTypes.NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output }
                };

                await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_GenerateInvoice", parameters);

                var invoiceId = parameters[9].Value != DBNull.Value ? Convert.ToInt64(parameters[9].Value) : 0;

                if (invoiceId > 0)
                {
                    // Log audit
                    await LogInvoiceAuditAsync(
                        invoiceId,
                        request.OrderId,
                        InvoiceAuditAction.GENERATED,
                        request.TriggeredBy,
                        request.TriggeredByType,
                        request.TriggerReason
                    );
                }

                return invoiceId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating invoice: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Creates a new invoice with manual data
        /// Used for custom invoice generation or admin overrides
        /// </summary>
        public async Task<long> CreateInvoiceAsync(CreateInvoiceDto invoice)
        {
            try
            {
                if (invoice == null)
                    throw new ArgumentNullException(nameof(invoice));

                if (invoice.LineItems == null || !invoice.LineItems.Any())
                    throw new ArgumentException("Invoice must have at least one line item");

                // For now, delegate to auto-generation
                // Full manual creation can be implemented later if needed
                var request = new InvoiceGenerationRequestDto
                {
                    OrderId = invoice.OrderId,
                    InvoiceType = invoice.InvoiceType,
                    TriggeredBy = invoice.CreatedBy,
                    TriggeredByType = InvoiceUserType.ADMIN
                };

                return await GenerateInvoiceAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating invoice: {ex.Message}", ex);
            }
        }

        #endregion

        #region Invoice Retrieval

        /// <summary>
        /// Gets complete invoice details by ID
        /// Includes line items, order summary, payment history
        /// </summary>
        public async Task<InvoiceDto?> GetInvoiceByIdAsync(long invoiceId)
        {
            try
            {
                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@InvoiceId", invoiceId)
                };

                var resultSets = await _dbHelper.ExecuteStoredProcedureMultipleAsync("sp_GetInvoiceById", parameters);

                if (resultSets == null || resultSets.Tables.Count == 0 || resultSets.Tables[0].Rows.Count == 0)
                    return null;

                var invoiceRow = resultSets.Tables[0].Rows[0];
                var invoice = MapInvoiceDto(invoiceRow);

                // Map line items if second result set exists
                if (resultSets.Tables.Count > 1 && resultSets.Tables[1].Rows.Count > 0)
                {
                    invoice.LineItems = MapLineItems(resultSets.Tables[1]);
                }

                // Map order summary
                invoice.OrderSummary = MapOrderSummary(invoiceRow);

                return invoice;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving invoice: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets invoice by invoice number
        /// </summary>
        public async Task<InvoiceDto?> GetInvoiceByNumberAsync(string invoiceNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(invoiceNumber))
                    throw new ArgumentException("Invoice number is required");

                var query = $@"
                    SELECT c_invoice_id FROM {Table.SysInvoice}
                    WHERE c_invoice_number = @InvoiceNumber AND c_is_deleted = FALSE
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@InvoiceNumber", invoiceNumber)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);

                if (dt.Rows.Count == 0)
                    return null;

                var invoiceId = Convert.ToInt64(dt.Rows[0]["c_invoice_id"]);
                return await GetInvoiceByIdAsync(invoiceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving invoice by number: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all invoices for a specific order
        /// PERFORMANCE FIX: Returns basic invoice summaries without line items to avoid N+1 queries
        /// For full invoice details including line items, call GetInvoiceByIdAsync for specific invoice
        /// </summary>
        public async Task<List<InvoiceDto>> GetInvoicesByOrderIdAsync(long orderId)
        {
            try
            {
                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId)
                };

                var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_GetInvoicesByOrderId", parameters);

                var invoices = new List<InvoiceDto>();

                // PERFORMANCE FIX: Map invoices directly from result set instead of N+1 queries
                // This returns basic invoice data without line items for faster dashboard loading
                foreach (DataRow row in dt.Rows)
                {
                    var invoice = new InvoiceDto
                    {
                        InvoiceId = Convert.ToInt64(row["InvoiceId"]),
                        OrderId = Convert.ToInt64(row["OrderId"]),
                        InvoiceNumber = row["InvoiceNumber"].ToString() ?? "",
                        InvoiceType = (InvoiceType)Convert.ToInt32(row["InvoiceType"]),
                        InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]),
                        DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : null,
                        Status = (InvoiceStatus)Convert.ToInt32(row["Status"]),
                        Subtotal = Convert.ToDecimal(row["Subtotal"]),
                        CgstAmount = Convert.ToDecimal(row["CgstAmount"]),
                        SgstAmount = Convert.ToDecimal(row["SgstAmount"]),
                        TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                        AmountPaid = Convert.ToDecimal(row["AmountPaid"]),
                        BalanceDue = Convert.ToDecimal(row["BalanceDue"]),
                        // Line items and payment history are not included in list view
                        // Call GetInvoiceByIdAsync if full details are needed
                        LineItems = new List<InvoiceLineItemDto>(),
                        PaymentHistory = new List<InvoicePaymentHistoryDto>()
                    };

                    invoices.Add(invoice);
                }

                return invoices;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving invoices for order: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets invoice by order ID and invoice type
        /// </summary>
        public async Task<InvoiceDto?> GetInvoiceByOrderAndTypeAsync(long orderId, InvoiceType invoiceType)
        {
            try
            {
                var query = $@"
                    SELECT c_invoice_id FROM {Table.SysInvoice}
                    WHERE c_orderid = @OrderId AND c_invoice_type = @InvoiceType AND c_is_deleted = FALSE
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@InvoiceType", (int)invoiceType)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);

                if (dt.Rows.Count == 0)
                    return null;

                var invoiceId = Convert.ToInt64(dt.Rows[0]["c_invoice_id"]);
                return await GetInvoiceByIdAsync(invoiceId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving invoice by order and type: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets paginated list of invoices with filters
        /// </summary>
        public async Task<InvoiceListResponseDto> GetInvoicesAsync(InvoiceListRequestDto request)
        {
            try
            {
                if (request == null)
                    request = new InvoiceListRequestDto();

                // Build dynamic query with filters
                var whereClause = "WHERE i.c_is_deleted = FALSE";
                var parameters = new List<NpgsqlParameter>();

                if (request.OrderId.HasValue)
                {
                    whereClause += " AND i.c_orderid = @OrderId";
                    parameters.Add(new NpgsqlParameter("@OrderId", request.OrderId.Value));
                }

                if (request.UserId.HasValue)
                {
                    whereClause += " AND i.c_userid = @UserId";
                    parameters.Add(new NpgsqlParameter("@UserId", request.UserId.Value));
                }

                if (request.CateringOwnerId.HasValue)
                {
                    whereClause += " AND i.c_ownerid = @OwnerId";
                    parameters.Add(new NpgsqlParameter("@OwnerId", request.CateringOwnerId.Value));
                }

                if (request.InvoiceType.HasValue)
                {
                    whereClause += " AND i.c_invoice_type = @InvoiceType";
                    parameters.Add(new NpgsqlParameter("@InvoiceType", (int)request.InvoiceType.Value));
                }

                if (request.Status.HasValue)
                {
                    whereClause += " AND i.c_status = @Status";
                    parameters.Add(new NpgsqlParameter("@Status", request.Status.Value.ToString()));
                }

                if (request.StartDate.HasValue)
                {
                    whereClause += " AND i.c_invoice_date >= @StartDate";
                    parameters.Add(new NpgsqlParameter("@StartDate", request.StartDate.Value));
                }

                if (request.EndDate.HasValue)
                {
                    whereClause += " AND i.c_invoice_date <= @EndDate";
                    parameters.Add(new NpgsqlParameter("@EndDate", request.EndDate.Value));
                }

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                {
                    whereClause += @" AND (i.c_invoice_number LIKE @SearchTerm
                                      OR o.c_order_number LIKE @SearchTerm
                                      OR u.c_fullname LIKE @SearchTerm)";
                    parameters.Add(new NpgsqlParameter("@SearchTerm", $"%{request.SearchTerm}%"));
                }

                if (request.MinAmount.HasValue)
                {
                    whereClause += " AND i.c_total_amount >= @MinAmount";
                    parameters.Add(new NpgsqlParameter("@MinAmount", request.MinAmount.Value));
                }

                if (request.MaxAmount.HasValue)
                {
                    whereClause += " AND i.c_total_amount <= @MaxAmount";
                    parameters.Add(new NpgsqlParameter("@MaxAmount", request.MaxAmount.Value));
                }

                if (request.IsOverdue.HasValue && request.IsOverdue.Value)
                {
                    whereClause += " AND i.c_status IN ('UNPAID', 'OVERDUE') AND i.c_due_date < NOW()";
                }

                if (request.IsPaid.HasValue && request.IsPaid.Value)
                {
                    whereClause += " AND i.c_status = 'PAID'";
                }

                // Count total
                var countQuery = $@"
                    SELECT COUNT(*) AS TotalCount
                    FROM {Table.SysInvoice} i
                    INNER JOIN {Table.SysOrders} o ON i.c_orderid = o.c_orderid
                    INNER JOIN {Table.SysUser} u ON i.c_userid = u.c_userid
                    {whereClause}
                ";

                var countDt = await _dbHelper.ExecuteAsync(countQuery, parameters.ToArray());
                var totalCount = Convert.ToInt32(countDt.Rows[0]["TotalCount"]);

                // Get paginated data
                var offset = (request.PageNumber - 1) * request.PageSize;
                var sortBy = ValidateSortColumn(request.SortBy);
                var sortOrder = request.SortOrder.ToUpper() == "ASC" ? "ASC" : "DESC";

                var dataQuery = $@"
                    SELECT
                        i.c_invoice_id AS InvoiceId,
                        i.c_invoice_number AS InvoiceNumber,
                        i.c_invoice_type AS InvoiceType,
                        i.c_invoice_date AS InvoiceDate,
                        i.c_due_date AS DueDate,
                        i.c_total_amount AS TotalAmount,
                        i.c_amount_paid AS AmountPaid,
                        i.c_balance_due AS BalanceDue,
                        i.c_status AS Status,
                        o.c_order_number AS OrderNumber,
                        u.c_fullname AS CustomerName,
                        CASE WHEN i.c_status IN ('UNPAID', 'OVERDUE') AND i.c_due_date < NOW() THEN 1 ELSE 0 END AS IsOverdue,
                        (i.c_due_date::date - CURRENT_DATE) AS DaysUntilDue
                    FROM {Table.SysInvoice} i
                    INNER JOIN {Table.SysOrders} o ON i.c_orderid = o.c_orderid
                    INNER JOIN {Table.SysUser} u ON i.c_userid = u.c_userid
                    {whereClause}
                    ORDER BY i.{sortBy} {sortOrder}
                    LIMIT {request.PageSize} OFFSET {offset}
                ";

                var dataDt = await _dbHelper.ExecuteAsync(dataQuery, parameters.ToArray());

                var invoices = new List<InvoiceSummaryDto>();
                foreach (DataRow row in dataDt.Rows)
                {
                    invoices.Add(MapInvoiceSummary(row));
                }

                var totalPages = (int)Math.Ceiling((double)totalCount / request.PageSize);

                return new InvoiceListResponseDto
                {
                    Invoices = invoices,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPreviousPage = request.PageNumber > 1,
                    HasNextPage = request.PageNumber < totalPages
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving invoice list: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all invoices for a user
        /// </summary>
        public async Task<List<InvoiceSummaryDto>> GetInvoicesByUserIdAsync(long userId)
        {
            var request = new InvoiceListRequestDto
            {
                UserId = userId,
                PageSize = 1000 // Get all
            };

            var response = await GetInvoicesAsync(request);
            return response.Invoices;
        }

        /// <summary>
        /// Gets all invoices for a catering owner
        /// </summary>
        public async Task<List<InvoiceSummaryDto>> GetInvoicesByOwnerIdAsync(long ownerId)
        {
            var request = new InvoiceListRequestDto
            {
                CateringOwnerId = ownerId,
                PageSize = 1000 // Get all
            };

            var response = await GetInvoicesAsync(request);
            return response.Invoices;
        }

        #endregion

        #region Invoice Update

        /// <summary>
        /// Updates invoice status
        /// </summary>
        public async Task<bool> UpdateInvoiceStatusAsync(long invoiceId, InvoiceStatus newStatus, string? remarks = null, long? updatedBy = null)
        {
            try
            {
                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@InvoiceId", invoiceId),
                    new NpgsqlParameter("@NewStatus", newStatus.ToString()),
                    new NpgsqlParameter("@Remarks", (object)remarks ?? DBNull.Value),
                    new NpgsqlParameter("@UpdatedBy", (object)updatedBy ?? DBNull.Value),
                    new NpgsqlParameter("@Success", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                    new NpgsqlParameter("@ErrorMessage", NpgsqlDbType.Varchar, 500) { Direction = ParameterDirection.Output }
                };

                await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_UpdateInvoiceStatus", parameters);

                var success = parameters[4].Value != DBNull.Value && (bool)parameters[4].Value;

                if (!success)
                {
                    var errorMessage = parameters[5].Value as string;
                    throw new InvalidOperationException(errorMessage ?? "Failed to update invoice status");
                }

                return success;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating invoice status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Links a Razorpay payment to an invoice
        /// </summary>
        public async Task<bool> LinkPaymentToInvoiceAsync(LinkPaymentToInvoiceDto paymentData)
        {
            try
            {
                if (paymentData == null)
                    throw new ArgumentNullException(nameof(paymentData));

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@InvoiceId", paymentData.InvoiceId),
                    new NpgsqlParameter("@RazorpayOrderId", paymentData.RazorpayOrderId),
                    new NpgsqlParameter("@RazorpayPaymentId", paymentData.RazorpayPaymentId),
                    new NpgsqlParameter("@AmountPaid", paymentData.AmountPaid),
                    new NpgsqlParameter("@PaymentMethod", paymentData.PaymentMethod),
                    new NpgsqlParameter("@TransactionId", (object)paymentData.TransactionId ?? DBNull.Value),
                    new NpgsqlParameter("@Success", NpgsqlTypes.NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                    new NpgsqlParameter("@ErrorMessage", NpgsqlTypes.NpgsqlDbType.Varchar, 500) { Direction = ParameterDirection.Output }
                };

                await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_LinkPaymentToInvoice", parameters);

                var success = parameters[6].Value != DBNull.Value && (bool)parameters[6].Value;

                if (!success)
                {
                    var errorMessage = parameters[7].Value as string;
                    throw new InvalidOperationException(errorMessage ?? "Failed to link payment to invoice");
                }

                return success;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error linking payment to invoice: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates PDF path after PDF generation
        /// </summary>
        public async Task<bool> UpdateInvoicePdfPathAsync(long invoiceId, string pdfPath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(pdfPath))
                    throw new ArgumentException("PDF path is required");

                var query = $@"
                    UPDATE {Table.SysInvoice}
                    SET c_pdf_path = @PdfPath,
                        c_pdf_generated_date = NOW(),
                        c_modifieddate = NOW()
                    WHERE c_invoice_id = @InvoiceId
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@InvoiceId", invoiceId),
                    new NpgsqlParameter("@PdfPath", pdfPath)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);

                if (rowsAffected > 0)
                {
                    // Get order ID for audit
                    var orderIdQuery = $"SELECT c_orderid FROM {Table.SysInvoice} WHERE c_invoice_id = @InvoiceId";
                    var dt = await _dbHelper.ExecuteAsync(orderIdQuery, new NpgsqlParameter[] { new NpgsqlParameter("@InvoiceId", invoiceId) });
                    var orderId = dt.Rows.Count > 0 ? Convert.ToInt64(dt.Rows[0]["c_orderid"]) : 0;

                    await LogInvoiceAuditAsync(invoiceId, orderId, InvoiceAuditAction.GENERATED, null, InvoiceUserType.SYSTEM, "PDF generated");
                }

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating invoice PDF path: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Recalculates final invoice after event completion
        /// </summary>
        public async Task<long> RecalculateFinalInvoiceAsync(long orderId)
        {
            try
            {
                // Get extra charges from order
                var query = $@"
                    SELECT c_extra_charges, c_final_guest_count, c_original_guest_count
                    FROM {Table.SysOrders}
                    WHERE c_orderid = @OrderId
                ";

                var dt = await _dbHelper.ExecuteAsync(query, new NpgsqlParameter[] { new NpgsqlParameter("@OrderId", orderId) });

                if (dt.Rows.Count == 0)
                    throw new Exception("Order not found");

                var extraCharges = dt.Rows[0]["c_extra_charges"] != DBNull.Value ? Convert.ToDecimal(dt.Rows[0]["c_extra_charges"]) : 0;
                var finalGuestCount = dt.Rows[0]["c_final_guest_count"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["c_final_guest_count"]) : 0;
                var originalGuestCount = dt.Rows[0]["c_original_guest_count"] != DBNull.Value ? Convert.ToInt32(dt.Rows[0]["c_original_guest_count"]) : 0;

                var extraGuestCount = Math.Max(0, finalGuestCount - originalGuestCount);

                // Check if FINAL invoice exists
                var existingInvoice = await GetInvoiceByOrderAndTypeAsync(orderId, InvoiceType.FINAL);

                if (existingInvoice != null)
                {
                    // Cancel existing and create new
                    await UpdateInvoiceStatusAsync(existingInvoice.InvoiceId, InvoiceStatus.CANCELLED, "Recalculated with updated charges");
                }

                // Generate new FINAL invoice
                var request = new InvoiceGenerationRequestDto
                {
                    OrderId = orderId,
                    InvoiceType = InvoiceType.FINAL,
                    ExtraGuestCount = extraGuestCount,
                    ExtraGuestCharges = extraCharges,
                    TriggeredByType = InvoiceUserType.SYSTEM
                };

                return await GenerateInvoiceAsync(request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error recalculating final invoice: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Regenerates an existing invoice (creates new version)
        /// </summary>
        public async Task<long> RegenerateInvoiceAsync(long invoiceId, string reason, long? regeneratedBy = null)
        {
            try
            {
                var existingInvoice = await GetInvoiceByIdAsync(invoiceId);
                if (existingInvoice == null)
                    throw new Exception("Invoice not found");

                // Cancel old invoice
                await UpdateInvoiceStatusAsync(invoiceId, InvoiceStatus.CANCELLED, $"Regenerated: {reason}", regeneratedBy);

                // Generate new invoice
                var request = new InvoiceGenerationRequestDto
                {
                    OrderId = existingInvoice.OrderId,
                    InvoiceType = existingInvoice.InvoiceType,
                    TriggeredBy = regeneratedBy,
                    TriggeredByType = InvoiceUserType.ADMIN,
                    TriggerReason = reason
                };

                var newInvoiceId = await GenerateInvoiceAsync(request);

                // Link new invoice to old one (version control)
                var updateQuery = $@"
                    UPDATE {Table.SysInvoice}
                    SET c_parent_invoice_id = @ParentInvoiceId
                    WHERE c_invoice_id = @NewInvoiceId
                ";

                await _dbHelper.ExecuteNonQueryAsync(updateQuery, new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@NewInvoiceId", newInvoiceId),
                    new NpgsqlParameter("@ParentInvoiceId", invoiceId)
                });

                return newInvoiceId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error regenerating invoice: {ex.Message}", ex);
            }
        }

        #endregion

        #region Payment Schedule

        /// <summary>
        /// Creates payment schedule for an order
        /// </summary>
        public async Task<bool> CreatePaymentScheduleAsync(long orderId, decimal totalAmount, DateTime eventDate)
        {
            try
            {
                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@TotalAmount", totalAmount),
                    new NpgsqlParameter("@EventDate", eventDate),
                    new NpgsqlParameter("@Success", NpgsqlTypes.NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output },
                    new NpgsqlParameter("@ErrorMessage", NpgsqlTypes.NpgsqlDbType.Varchar, 500) { Direction = ParameterDirection.Output }
                };

                await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_CreatePaymentSchedule", parameters);

                var success = parameters[3].Value != DBNull.Value && (bool)parameters[3].Value;

                return success;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating payment schedule: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets complete payment schedule for an order
        /// </summary>
        public async Task<PaymentScheduleDto?> GetPaymentScheduleAsync(long orderId)
        {
            try
            {
                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId)
                };

                var resultSets = await _dbHelper.ExecuteStoredProcedureMultipleAsync("sp_GetPaymentSchedule", parameters);

                if (resultSets == null || resultSets.Tables.Count == 0 || resultSets.Tables[0].Rows.Count == 0)
                    return null;

                var summaryRow = resultSets.Tables[0].Rows[0];
                var schedule = new PaymentScheduleDto
                {
                    OrderId = Convert.ToInt64(summaryRow["OrderId"]),
                    OrderNumber = summaryRow["OrderNumber"].ToString() ?? string.Empty,
                    TotalOrderAmount = Convert.ToDecimal(summaryRow["TotalOrderAmount"]),
                    TotalPaidAmount = Convert.ToDecimal(summaryRow["TotalPaidAmount"]),
                    TotalPendingAmount = Convert.ToDecimal(summaryRow["TotalPendingAmount"]),
                    PaymentProgressPercentage = Convert.ToDecimal(summaryRow["PaymentProgressPercentage"])
                };

                // Map stages
                if (resultSets.Tables.Count > 1)
                {
                    foreach (DataRow row in resultSets.Tables[1].Rows)
                    {
                        schedule.Stages.Add(MapPaymentStageDto(row));
                    }
                }

                return schedule;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment schedule: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets specific payment stage
        /// </summary>
        public async Task<PaymentStageDto?> GetPaymentStageAsync(long orderId, PaymentStageType stageType)
        {
            try
            {
                var schedule = await GetPaymentScheduleAsync(orderId);
                return schedule?.Stages.FirstOrDefault(s => s.StageType == stageType);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving payment stage: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates payment stage status after invoice payment
        /// </summary>
        public async Task<bool> UpdatePaymentStageStatusAsync(long orderId, PaymentStageType stageType, long invoiceId, PaymentScheduleStatus status)
        {
            try
            {
                var query = $@"
                    UPDATE {Table.SysPaymentSchedule}
                    SET c_status = @Status,
                        c_invoice_id = @InvoiceId,
                        c_modifieddate = NOW()
                    WHERE c_orderid = @OrderId AND c_stage_type = @StageType
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@StageType", stageType.ToString()),
                    new NpgsqlParameter("@InvoiceId", invoiceId),
                    new NpgsqlParameter("@Status", status.ToString())
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating payment stage status: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all orders with invoices due for auto-generation
        /// </summary>
        public async Task<List<long>> GetOrdersForAutoInvoiceGenerationAsync()
        {
            try
            {
                var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_GetOrdersForAutoInvoiceGeneration", Array.Empty<NpgsqlParameter>());

                var orderIds = new List<long>();
                foreach (DataRow row in dt.Rows)
                {
                    orderIds.Add(Convert.ToInt64(row["OrderId"]));
                }

                return orderIds;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving orders for auto-generation: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Updates reminder sent count for a payment stage
        /// </summary>
        public async Task<bool> UpdatePaymentReminderSentAsync(long scheduleId)
        {
            try
            {
                var query = $@"
                    UPDATE {Table.SysPaymentSchedule}
                    SET c_reminder_sent_count = c_reminder_sent_count + 1,
                        c_last_reminder_date = NOW()
                    WHERE c_schedule_id = @ScheduleId
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@ScheduleId", scheduleId)
                };

                var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating reminder count: {ex.Message}", ex);
            }
        }

        #endregion

        #region Invoice Audit

        /// <summary>
        /// Logs an audit entry for invoice action
        /// </summary>
        public async Task<bool> LogInvoiceAuditAsync(
            long invoiceId,
            long orderId,
            InvoiceAuditAction action,
            long? performedBy = null,
            InvoiceUserType? performedByType = null,
            string? remarks = null,
            InvoiceStatus? oldStatus = null,
            InvoiceStatus? newStatus = null,
            string? ipAddress = null,
            string? userAgent = null)
        {
            try
            {
                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@InvoiceId", invoiceId),
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@Action", action.ToString()),
                    new NpgsqlParameter("@PerformedBy", (object)performedBy ?? DBNull.Value),
                    new NpgsqlParameter("@PerformedByType", performedByType?.ToString() ?? "SYSTEM"),
                    new NpgsqlParameter("@Remarks", (object)remarks ?? DBNull.Value),
                    new NpgsqlParameter("@OldStatus", oldStatus.HasValue ? oldStatus.Value.ToString() : (object)DBNull.Value),
                    new NpgsqlParameter("@NewStatus", newStatus.HasValue ? newStatus.Value.ToString() : (object)DBNull.Value),
                    new NpgsqlParameter("@IpAddress", (object)ipAddress ?? DBNull.Value),
                    new NpgsqlParameter("@UserAgent", (object)userAgent ?? DBNull.Value)
                };

                await _dbHelper.ExecuteStoredProcedureAsync<dynamic>("sp_LogInvoiceAudit", parameters);
                return true;
            }
            catch (Exception ex)
            {
                // Log audit errors should not break the main flow
                Console.WriteLine($"Audit logging error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Gets audit log for an invoice
        /// </summary>
        public async Task<List<InvoiceAuditLogDto>> GetInvoiceAuditLogAsync(long invoiceId)
        {
            try
            {
                var query = $@"
                    SELECT
                        c_audit_id AS AuditId,
                        c_invoice_id AS InvoiceId,
                        c_orderid AS OrderId,
                        c_action AS Action,
                        c_performed_by AS PerformedBy,
                        c_performed_by_type AS PerformedByType,
                        c_old_status AS OldStatus,
                        c_new_status AS NewStatus,
                        c_old_amount_paid AS OldAmountPaid,
                        c_new_amount_paid AS NewAmountPaid,
                        c_ip_address AS IpAddress,
                        c_user_agent AS UserAgent,
                        c_remarks AS Remarks,
                        c_timestamp AS Timestamp
                    FROM {Table.SysInvoiceAuditLog}
                    WHERE c_invoice_id = @InvoiceId
                    ORDER BY c_timestamp DESC
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@InvoiceId", invoiceId)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);

                var logs = new List<InvoiceAuditLogDto>();
                foreach (DataRow row in dt.Rows)
                {
                    logs.Add(MapAuditLogDto(row));
                }

                return logs;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving audit log: {ex.Message}", ex);
            }
        }

        #endregion

        #region Statistics & Reports

        /// <summary>
        /// Gets invoice statistics for dashboard
        /// </summary>
        public async Task<InvoiceStatisticsDto> GetInvoiceStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null, long? ownerId = null)
        {
            try
            {
                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@StartDate", (object)startDate ?? DBNull.Value),
                    new NpgsqlParameter("@EndDate", (object)endDate ?? DBNull.Value),
                    new NpgsqlParameter("@OwnerId", (object)ownerId ?? DBNull.Value)
                };

                var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_GetInvoiceStatistics", parameters);

                if (dt.Rows.Count == 0)
                    return new InvoiceStatisticsDto();

                var row = dt.Rows[0];
                return new InvoiceStatisticsDto
                {
                    TotalInvoices = Convert.ToInt32(row["TotalInvoices"]),
                    UnpaidInvoices = Convert.ToInt32(row["UnpaidInvoices"]),
                    PaidInvoices = Convert.ToInt32(row["PaidInvoices"]),
                    OverdueInvoices = Convert.ToInt32(row["OverdueInvoices"]),
                    TotalInvoiceAmount = Convert.ToDecimal(row["TotalInvoiceAmount"]),
                    TotalPaidAmount = Convert.ToDecimal(row["TotalPaidAmount"]),
                    TotalPendingAmount = Convert.ToDecimal(row["TotalPendingAmount"]),
                    TotalOverdueAmount = Convert.ToDecimal(row["TotalOverdueAmount"]),
                    BookingInvoiceCount = Convert.ToInt32(row["BookingInvoiceCount"]),
                    PreEventInvoiceCount = Convert.ToInt32(row["PreEventInvoiceCount"]),
                    FinalInvoiceCount = Convert.ToInt32(row["FinalInvoiceCount"]),
                    AverageInvoiceAmount = Convert.ToDecimal(row["AverageInvoiceAmount"]),
                    PaymentSuccessRate = Convert.ToDecimal(row["PaymentSuccessRate"])
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving invoice statistics: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets all overdue invoices
        /// </summary>
        public async Task<List<InvoiceSummaryDto>> GetOverdueInvoicesAsync()
        {
            try
            {
                var dt = await _dbHelper.ExecuteStoredProcedureAsync<DataTable>("sp_GetOverdueInvoices", Array.Empty<NpgsqlParameter>());

                var invoices = new List<InvoiceSummaryDto>();
                foreach (DataRow row in dt.Rows)
                {
                    invoices.Add(MapInvoiceSummary(row));
                }

                return invoices;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving overdue invoices: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets invoices due within X days
        /// </summary>
        public async Task<List<InvoiceSummaryDto>> GetInvoicesDueSoonAsync(int daysAhead = 3)
        {
            try
            {
                var query = $@"
                    SELECT
                        i.c_invoice_id AS InvoiceId,
                        i.c_invoice_number AS InvoiceNumber,
                        i.c_invoice_type AS InvoiceType,
                        i.c_invoice_date AS InvoiceDate,
                        i.c_due_date AS DueDate,
                        i.c_total_amount AS TotalAmount,
                        i.c_amount_paid AS AmountPaid,
                        i.c_balance_due AS BalanceDue,
                        i.c_status AS Status,
                        o.c_order_number AS OrderNumber,
                        u.c_fullname AS CustomerName,
                        0 AS IsOverdue,
                        (i.c_due_date::date - CURRENT_DATE) AS DaysUntilDue
                    FROM {Table.SysInvoice} i
                    INNER JOIN {Table.SysOrders} o ON i.c_orderid = o.c_orderid
                    INNER JOIN {Table.SysUser} u ON i.c_userid = u.c_userid
                    WHERE i.c_status IN ('UNPAID', 'PARTIALLY_PAID')
                        AND i.c_due_date BETWEEN NOW() AND (NOW() + (@DaysAhead * INTERVAL '1 day'))
                        AND i.c_is_deleted = FALSE
                    ORDER BY i.c_due_date
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@DaysAhead", daysAhead)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);

                var invoices = new List<InvoiceSummaryDto>();
                foreach (DataRow row in dt.Rows)
                {
                    invoices.Add(MapInvoiceSummary(row));
                }

                return invoices;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving invoices due soon: {ex.Message}", ex);
            }
        }

        #endregion

        #region Validation & Checks

        /// <summary>
        /// Checks if invoice exists for order and type
        /// </summary>
        public async Task<bool> InvoiceExistsAsync(long orderId, InvoiceType invoiceType)
        {
            try
            {
                var query = $@"
                    SELECT COUNT(*) FROM {Table.SysInvoice}
                    WHERE c_orderid = @OrderId AND c_invoice_type = @InvoiceType AND c_is_deleted = FALSE
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId),
                    new NpgsqlParameter("@InvoiceType", (int)invoiceType)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);
                return Convert.ToInt32(dt.Rows[0][0]) > 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking invoice existence: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates if invoice can be paid (status check)
        /// </summary>
        public async Task<bool> CanPayInvoiceAsync(long invoiceId)
        {
            try
            {
                var query = $@"
                    SELECT c_status FROM {Table.SysInvoice}
                    WHERE c_invoice_id = @InvoiceId AND c_is_deleted = FALSE
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@InvoiceId", invoiceId)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);

                if (dt.Rows.Count == 0)
                    return false;

                var status = dt.Rows[0]["c_status"].ToString();
                return status == "UNPAID" || status == "PARTIALLY_PAID" || status == "OVERDUE";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if invoice can be paid: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validates if next stage invoice can be generated
        /// </summary>
        public async Task<bool> CanGenerateNextStageInvoiceAsync(long orderId, InvoiceType nextStage)
        {
            try
            {
                // BOOKING can always be generated
                if (nextStage == InvoiceType.BOOKING)
                    return true;

                // PRE_EVENT requires BOOKING to be paid
                if (nextStage == InvoiceType.PRE_EVENT)
                {
                    var bookingInvoice = await GetInvoiceByOrderAndTypeAsync(orderId, InvoiceType.BOOKING);
                    return bookingInvoice != null && bookingInvoice.Status == InvoiceStatus.PAID;
                }

                // FINAL requires PRE_EVENT to be paid
                if (nextStage == InvoiceType.FINAL)
                {
                    var preEventInvoice = await GetInvoiceByOrderAndTypeAsync(orderId, InvoiceType.PRE_EVENT);
                    return preEventInvoice != null && preEventInvoice.Status == InvoiceStatus.PAID;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error checking if next stage can be generated: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets total amount paid for an order across all invoices
        /// </summary>
        public async Task<decimal> GetTotalPaidAmountAsync(long orderId)
        {
            try
            {
                var query = $@"
                    SELECT COALESCE(SUM(c_amount_paid), 0) AS TotalPaid
                    FROM {Table.SysInvoice}
                    WHERE c_orderid = @OrderId AND c_is_deleted = FALSE
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);
                return Convert.ToDecimal(dt.Rows[0]["TotalPaid"]);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting total paid amount: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets payment progress percentage for an order
        /// </summary>
        public async Task<decimal> GetPaymentProgressPercentageAsync(long orderId)
        {
            try
            {
                var query = $@"
                    SELECT c_payment_progress_percentage
                    FROM {Table.SysOrders}
                    WHERE c_orderid = @OrderId
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@OrderId", orderId)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);

                if (dt.Rows.Count == 0)
                    return 0;

                return dt.Rows[0]["c_payment_progress_percentage"] != DBNull.Value
                    ? Convert.ToDecimal(dt.Rows[0]["c_payment_progress_percentage"])
                    : 0;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting payment progress: {ex.Message}", ex);
            }
        }

        #endregion

        #region Helper Methods

        private InvoiceDto MapInvoiceDto(DataRow row)
        {
            return new InvoiceDto
            {
                InvoiceId = Convert.ToInt64(row["InvoiceId"]),
                OrderId = Convert.ToInt64(row["OrderId"]),
                EventId = row["EventId"] != DBNull.Value ? Convert.ToInt64(row["EventId"]) : null,
                UserId = Convert.ToInt64(row["UserId"]),
                CateringOwnerId = Convert.ToInt64(row["CateringOwnerId"]),
                InvoiceType = (InvoiceType)Convert.ToInt32(row["InvoiceType"]),
                IsProforma = Convert.ToBoolean(row["IsProforma"]),
                InvoiceNumber = row["InvoiceNumber"].ToString() ?? string.Empty,
                InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]),
                DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : null,
                Subtotal = Convert.ToDecimal(row["Subtotal"]),
                CgstPercent = Convert.ToDecimal(row["CgstPercent"]),
                SgstPercent = Convert.ToDecimal(row["SgstPercent"]),
                CgstAmount = Convert.ToDecimal(row["CgstAmount"]),
                SgstAmount = Convert.ToDecimal(row["SgstAmount"]),
                TotalTaxAmount = Convert.ToDecimal(row["TotalTaxAmount"]),
                DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                AmountPaid = Convert.ToDecimal(row["AmountPaid"]),
                BalanceDue = Convert.ToDecimal(row["BalanceDue"]),
                PaymentStageType = Enum.Parse<PaymentStageType>(row["PaymentStageType"].ToString() ?? "BOOKING"),
                PaymentPercentage = Convert.ToDecimal(row["PaymentPercentage"]),
                Status = Enum.Parse<InvoiceStatus>(row["Status"].ToString() ?? "UNPAID"),
                RazorpayOrderId = row["RazorpayOrderId"]?.ToString(),
                RazorpayPaymentId = row["RazorpayPaymentId"]?.ToString(),
                TransactionId = row["TransactionId"]?.ToString(),
                PaymentMethod = row["PaymentMethod"]?.ToString(),
                PaymentDate = row["PaymentDate"] != DBNull.Value ? Convert.ToDateTime(row["PaymentDate"]) : null,
                CompanyGstin = row["CompanyGstin"]?.ToString(),
                CustomerGstin = row["CustomerGstin"]?.ToString(),
                PlaceOfSupply = row["PlaceOfSupply"]?.ToString(),
                SacCode = row["SacCode"]?.ToString() ?? "996331",
                Notes = row["Notes"]?.ToString(),
                TermsAndConditions = row["TermsAndConditions"]?.ToString(),
                InternalRemarks = row["InternalRemarks"]?.ToString(),
                PdfPath = row["PdfPath"]?.ToString(),
                PdfGeneratedDate = row["PdfGeneratedDate"] != DBNull.Value ? Convert.ToDateTime(row["PdfGeneratedDate"]) : null,
                CreatedBy = row["CreatedBy"] != DBNull.Value ? Convert.ToInt64(row["CreatedBy"]) : null,
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                ModifiedBy = row["ModifiedBy"] != DBNull.Value ? Convert.ToInt64(row["ModifiedBy"]) : null,
                ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null,
                Version = row["Version"] != DBNull.Value ? Convert.ToInt32(row["Version"]) : 1,
                ParentInvoiceId = row["ParentInvoiceId"] != DBNull.Value ? Convert.ToInt64(row["ParentInvoiceId"]) : null
            };
        }

        private List<InvoiceLineItemDto> MapLineItems(DataTable dt)
        {
            var lineItems = new List<InvoiceLineItemDto>();

            foreach (DataRow row in dt.Rows)
            {
                lineItems.Add(new InvoiceLineItemDto
                {
                    LineItemId = Convert.ToInt64(row["LineItemId"]),
                    InvoiceId = Convert.ToInt64(row["InvoiceId"]),
                    ItemType = Enum.Parse<InvoiceLineItemType>(row["ItemType"].ToString() ?? "OTHER"),
                    ItemId = row["ItemId"] != DBNull.Value ? Convert.ToInt64(row["ItemId"]) : null,
                    Description = row["Description"].ToString() ?? string.Empty,
                    HsnSacCode = row["HsnSacCode"]?.ToString(),
                    Quantity = Convert.ToDecimal(row["Quantity"]),
                    UnitOfMeasure = row["UnitOfMeasure"]?.ToString(),
                    UnitPrice = Convert.ToDecimal(row["UnitPrice"]),
                    Subtotal = Convert.ToDecimal(row["Subtotal"]),
                    TaxPercent = Convert.ToDecimal(row["TaxPercent"]),
                    CgstPercent = Convert.ToDecimal(row["CgstPercent"]),
                    SgstPercent = Convert.ToDecimal(row["SgstPercent"]),
                    TaxAmount = Convert.ToDecimal(row["TaxAmount"]),
                    CgstAmount = Convert.ToDecimal(row["CgstAmount"]),
                    SgstAmount = Convert.ToDecimal(row["SgstAmount"]),
                    DiscountPercent = Convert.ToDecimal(row["DiscountPercent"]),
                    DiscountAmount = Convert.ToDecimal(row["DiscountAmount"]),
                    Total = Convert.ToDecimal(row["Total"]),
                    Sequence = Convert.ToInt32(row["Sequence"]),
                    CreatedDate = Convert.ToDateTime(row["CreatedDate"])
                });
            }

            return lineItems;
        }

        private InvoiceOrderSummaryDto MapOrderSummary(DataRow row)
        {
            return new InvoiceOrderSummaryDto
            {
                OrderId = Convert.ToInt64(row["OrderId"]),
                OrderNumber = row["OrderNumber"].ToString() ?? string.Empty,
                EventDate = Convert.ToDateTime(row["EventDate"]),
                EventTime = row["EventTime"].ToString() ?? string.Empty,
                EventType = row["EventType"].ToString() ?? string.Empty,
                EventLocation = row["EventLocation"].ToString() ?? string.Empty,
                GuestCount = Convert.ToInt32(row["GuestCount"]),
                OriginalGuestCount = row["OriginalGuestCount"] != DBNull.Value ? Convert.ToInt32(row["OriginalGuestCount"]) : null,
                FinalGuestCount = row["FinalGuestCount"] != DBNull.Value ? Convert.ToInt32(row["FinalGuestCount"]) : null,
                GuestCountLocked = Convert.ToBoolean(row["GuestCountLocked"]),
                MenuLocked = Convert.ToBoolean(row["MenuLocked"]),
                CustomerName = row["CustomerName"].ToString() ?? string.Empty,
                CustomerPhone = row["CustomerPhone"].ToString() ?? string.Empty,
                CustomerEmail = row["CustomerEmail"].ToString() ?? string.Empty,
                PartnerName = row["PartnerName"].ToString() ?? string.Empty,
                PartnerPhone = row["PartnerPhone"].ToString() ?? string.Empty,
                PartnerEmail = row["PartnerEmail"].ToString() ?? string.Empty
            };
        }

        private InvoiceSummaryDto MapInvoiceSummary(DataRow row)
        {
            return new InvoiceSummaryDto
            {
                InvoiceId = Convert.ToInt64(row["InvoiceId"]),
                InvoiceNumber = row["InvoiceNumber"].ToString() ?? string.Empty,
                InvoiceType = (InvoiceType)Convert.ToInt32(row["InvoiceType"]),
                InvoiceDate = Convert.ToDateTime(row["InvoiceDate"]),
                DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : null,
                TotalAmount = Convert.ToDecimal(row["TotalAmount"]),
                AmountPaid = Convert.ToDecimal(row["AmountPaid"]),
                BalanceDue = Convert.ToDecimal(row["BalanceDue"]),
                Status = Enum.Parse<InvoiceStatus>(row["Status"].ToString() ?? "UNPAID"),
                OrderNumber = row["OrderNumber"].ToString() ?? string.Empty,
                CustomerName = row["CustomerName"].ToString() ?? string.Empty,
                IsOverdue = row["IsOverdue"] != DBNull.Value && Convert.ToBoolean(row["IsOverdue"]),
                DaysUntilDue = row["DaysUntilDue"] != DBNull.Value ? Convert.ToInt32(row["DaysUntilDue"]) : null
            };
        }

        private PaymentStageDto MapPaymentStageDto(DataRow row)
        {
            return new PaymentStageDto
            {
                ScheduleId = Convert.ToInt64(row["ScheduleId"]),
                OrderId = Convert.ToInt64(row["OrderId"]),
                StageType = Enum.Parse<PaymentStageType>(row["StageType"].ToString() ?? "BOOKING"),
                StageSequence = Convert.ToInt32(row["StageSequence"]),
                Percentage = Convert.ToDecimal(row["Percentage"]),
                Amount = Convert.ToDecimal(row["Amount"]),
                DueDate = row["DueDate"] != DBNull.Value ? Convert.ToDateTime(row["DueDate"]) : null,
                TriggerEvent = Enum.Parse<PaymentTriggerEvent>(row["TriggerEvent"].ToString() ?? "ORDER_APPROVED"),
                AutoGenerateDate = row["AutoGenerateDate"] != DBNull.Value ? Convert.ToDateTime(row["AutoGenerateDate"]) : null,
                InvoiceId = row["InvoiceId"] != DBNull.Value ? Convert.ToInt64(row["InvoiceId"]) : null,
                Status = Enum.Parse<PaymentScheduleStatus>(row["Status"].ToString() ?? "PENDING"),
                ReminderSentCount = Convert.ToInt32(row["ReminderSentCount"]),
                LastReminderDate = row["LastReminderDate"] != DBNull.Value ? Convert.ToDateTime(row["LastReminderDate"]) : null,
                NextReminderDate = row["NextReminderDate"] != DBNull.Value ? Convert.ToDateTime(row["NextReminderDate"]) : null,
                CreatedDate = Convert.ToDateTime(row["CreatedDate"]),
                ModifiedDate = row["ModifiedDate"] != DBNull.Value ? Convert.ToDateTime(row["ModifiedDate"]) : null
            };
        }

        private InvoiceAuditLogDto MapAuditLogDto(DataRow row)
        {
            return new InvoiceAuditLogDto
            {
                AuditId = Convert.ToInt64(row["AuditId"]),
                InvoiceId = Convert.ToInt64(row["InvoiceId"]),
                OrderId = Convert.ToInt64(row["OrderId"]),
                Action = Enum.Parse<InvoiceAuditAction>(row["Action"].ToString() ?? "GENERATED"),
                PerformedBy = row["PerformedBy"] != DBNull.Value ? Convert.ToInt64(row["PerformedBy"]) : null,
                PerformedByType = row["PerformedByType"] != DBNull.Value
                    ? Enum.Parse<InvoiceUserType>(row["PerformedByType"].ToString() ?? "SYSTEM")
                    : null,
                OldStatus = row["OldStatus"] != DBNull.Value
                    ? Enum.Parse<InvoiceStatus>(row["OldStatus"].ToString() ?? "UNPAID")
                    : null,
                NewStatus = row["NewStatus"] != DBNull.Value
                    ? Enum.Parse<InvoiceStatus>(row["NewStatus"].ToString() ?? "UNPAID")
                    : null,
                OldAmountPaid = row["OldAmountPaid"] != DBNull.Value ? Convert.ToDecimal(row["OldAmountPaid"]) : null,
                NewAmountPaid = row["NewAmountPaid"] != DBNull.Value ? Convert.ToDecimal(row["NewAmountPaid"]) : null,
                IpAddress = row["IpAddress"]?.ToString(),
                UserAgent = row["UserAgent"]?.ToString(),
                Remarks = row["Remarks"]?.ToString(),
                Timestamp = Convert.ToDateTime(row["Timestamp"])
            };
        }

        private string ValidateSortColumn(string sortBy)
        {
            var validColumns = new[] { "c_invoice_date", "c_total_amount", "c_due_date", "c_invoice_number", "c_status" };
            var column = "c_" + sortBy.ToLower().Replace("invoicedate", "invoice_date")
                                              .Replace("totalamount", "total_amount")
                                              .Replace("duedate", "due_date")
                                              .Replace("invoicenumber", "invoice_number")
                                              .Replace("status", "status");

            return validColumns.Contains(column) ? column : "c_invoice_date";
        }

        #endregion

        #region Background Job Helper Methods

        /// <summary>
        /// Gets all overdue invoices for background job processing (unpaid and past due date)
        /// Used by background job to mark invoices as overdue
        /// </summary>
        public async Task<List<InvoiceDto>> GetOverdueInvoicesForJobAsync()
        {
            try
            {
                var query = $@"
                    SELECT i.*,
                           o.c_userid AS UserId,
                           o.c_ownerid AS CateringOwnerId
                    FROM {Table.SysInvoice} i
                    INNER JOIN {Table.SysOrders} o ON i.c_orderid = o.c_orderid
                    WHERE i.c_status = 1 -- UNPAID
                      AND i.c_due_date < NOW()
                      AND i.c_is_deleted = FALSE
                    ORDER BY i.c_due_date ASC
                ";

                var dt = await _dbHelper.ExecuteAsync(query, Array.Empty<NpgsqlParameter>());
                return dt.AsEnumerable().Select(MapInvoiceDto).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting overdue invoices: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets orders approaching guest lock date (5 days before event)
        /// Used by background job to auto-lock guest counts
        /// </summary>
        public async Task<List<dynamic>> GetOrdersApproachingGuestLockAsync()
        {
            try
            {
                var query = $@"
                    SELECT c_orderid AS OrderId,
                           c_event_date AS EventDate,
                           ((c_event_date::date - 5) - CURRENT_DATE) AS DaysUntilLock
                    FROM {Table.SysOrders}
                    WHERE c_guest_count_locked = FALSE
                      AND c_event_date > NOW()
                      AND ((c_event_date::date - 5) - CURRENT_DATE) <= 1
                      AND c_order_status >= 6 -- BOOKING_PAID or later
                      AND c_is_deleted = FALSE
                    ORDER BY c_event_date ASC
                ";

                var dt = await _dbHelper.ExecuteAsync(query, Array.Empty<NpgsqlParameter>());
                return dt.AsEnumerable().Select(row => new
                {
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    EventDate = Convert.ToDateTime(row["EventDate"]),
                    DaysUntilLock = Convert.ToInt32(row["DaysUntilLock"])
                }).Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting orders approaching guest lock: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets orders approaching menu lock date (3 days before event)
        /// Used by background job to auto-lock menus
        /// </summary>
        public async Task<List<dynamic>> GetOrdersApproachingMenuLockAsync()
        {
            try
            {
                var query = $@"
                    SELECT c_orderid AS OrderId,
                           c_event_date AS EventDate,
                           ((c_event_date::date - 3) - CURRENT_DATE) AS DaysUntilLock
                    FROM {Table.SysOrders}
                    WHERE c_menu_locked = FALSE
                      AND c_event_date > NOW()
                      AND ((c_event_date::date - 3) - CURRENT_DATE) <= 1
                      AND c_order_status >= 6 -- BOOKING_PAID or later
                      AND c_is_deleted = FALSE
                    ORDER BY c_event_date ASC
                ";

                var dt = await _dbHelper.ExecuteAsync(query, Array.Empty<NpgsqlParameter>());
                return dt.AsEnumerable().Select(row => new
                {
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    EventDate = Convert.ToDateTime(row["EventDate"]),
                    DaysUntilLock = Convert.ToInt32(row["DaysUntilLock"])
                }).Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting orders approaching menu lock: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets pending invoices due within specified days
        /// Used by background job to send payment reminders
        /// </summary>
        public async Task<List<InvoiceDto>> GetPendingInvoicesDueWithinDaysAsync(int daysThreshold)
        {
            try
            {
                var query = $@"
                    SELECT i.*,
                           o.c_userid AS UserId,
                           o.c_ownerid AS CateringOwnerId
                    FROM {Table.SysInvoice} i
                    INNER JOIN {Table.SysOrders} o ON i.c_orderid = o.c_orderid
                    WHERE i.c_status IN (1, 2) -- UNPAID or OVERDUE
                      AND i.c_due_date IS NOT NULL
                      AND (i.c_due_date::date - CURRENT_DATE) BETWEEN 0 AND @DaysThreshold
                      AND i.c_is_deleted = FALSE
                    ORDER BY i.c_due_date ASC
                ";

                var parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@DaysThreshold", daysThreshold)
                };

                var dt = await _dbHelper.ExecuteAsync(query, parameters);
                return dt.AsEnumerable().Select(MapInvoiceDto).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting pending invoices: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets orders ready for PRE_EVENT invoice generation
        /// Orders with BOOKING_PAID status approaching guest lock date
        /// </summary>
        public async Task<List<dynamic>> GetOrdersReadyForPreEventInvoiceAsync()
        {
            try
            {
                var query = $@"
                    SELECT c_orderid AS OrderId,
                           c_event_date AS EventDate,
                           c_order_total AS OrderTotal
                    FROM {Table.SysOrders}
                    WHERE c_order_status = 6 -- BOOKING_PAID
                      AND c_event_date > NOW()
                      AND (c_event_date::date - CURRENT_DATE) <= 5 -- Within 5 days of event
                      AND c_is_deleted = FALSE
                      AND NOT EXISTS (
                          SELECT 1 FROM {Table.SysInvoice}
                          WHERE c_orderid = {Table.SysOrders}.c_orderid
                            AND c_invoice_type = 2 -- PRE_EVENT
                            AND c_is_deleted = FALSE
                      )
                    ORDER BY c_event_date ASC
                ";

                var dt = await _dbHelper.ExecuteAsync(query, Array.Empty<NpgsqlParameter>());
                return dt.AsEnumerable().Select(row => new
                {
                    OrderId = Convert.ToInt64(row["OrderId"]),
                    EventDate = Convert.ToDateTime(row["EventDate"]),
                    OrderTotal = Convert.ToDecimal(row["OrderTotal"])
                }).Cast<dynamic>().ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting orders ready for PRE_EVENT invoice: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Gets paginated invoices for a user with filters
        /// </summary>
        public async Task<InvoiceListResponseDto> GetInvoicesByUserAsync(long userId, int pageNumber, int pageSize, InvoiceStatus? status, InvoiceType? type)
        {
            var request = new InvoiceListRequestDto
            {
                UserId = userId,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Status = status,
                InvoiceType = type
            };

            return await GetInvoicesAsync(request);
        }

        /// <summary>
        /// Gets invoice statistics for a specific order
        /// </summary>
        public async Task<object> GetOrderInvoiceStatsAsync(long orderId)
        {
            try
            {
                var invoices = await GetInvoicesByOrderIdAsync(orderId);
                var schedule = await GetPaymentScheduleAsync(orderId);

                return new
                {
                    OrderId = orderId,
                    TotalInvoices = invoices.Count,
                    TotalAmount = invoices.Sum(i => i.TotalAmount),
                    TotalPaid = invoices.Sum(i => i.AmountPaid),
                    TotalDue = invoices.Sum(i => i.BalanceDue),
                    PaymentProgress = schedule?.PaymentProgressPercentage ?? 0,
                    Invoices = invoices
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting order invoice stats: {ex.Message}", ex);
            }
        }

        #endregion
    }
}

