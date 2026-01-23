using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Models.Owner;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Common
{
    public class OrderModificationRepository
    {
        private readonly SqlDatabaseManager _db;

        public OrderModificationRepository(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        // ===================================
        // INSERT ORDER MODIFICATION
        // ===================================
        public async Task<long> InsertOrderModificationAsync(CreateOrderModificationDto modificationData)
        {
            try
            {
                StringBuilder query = new StringBuilder();
                query.Append($@"
                    INSERT INTO {Table.SysOrderModifications} (
                        c_orderid, c_modification_type, c_original_guest_count, c_modified_guest_count,
                        c_additional_amount, c_modification_reason, c_requested_by, c_status, c_created_date
                    ) VALUES (
                        @OrderId, @ModificationType, @OriginalGuestCount, @ModifiedGuestCount,
                        @AdditionalAmount, @ModificationReason, @RequestedBy, 'Pending', GETDATE()
                    );
                    SELECT CAST(SCOPE_IDENTITY() AS BIGINT);
                ");

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", modificationData.OrderId),
                    new SqlParameter("@ModificationType", modificationData.ModificationType),
                    new SqlParameter("@OriginalGuestCount", (object)modificationData.OriginalGuestCount ?? DBNull.Value),
                    new SqlParameter("@ModifiedGuestCount", (object)modificationData.ModifiedGuestCount ?? DBNull.Value),
                    new SqlParameter("@AdditionalAmount", modificationData.AdditionalAmount),
                    new SqlParameter("@ModificationReason", modificationData.ModificationReason),
                    new SqlParameter("@RequestedBy", modificationData.RequestedBy)
                };

                DataTable dt = await _db.ExecuteAsync(query.ToString(), parameters);
                if (dt.Rows.Count > 0)
                {
                    return Convert.ToInt64(dt.Rows[0][0]);
                }

                return 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error inserting order modification: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET ORDER MODIFICATIONS
        // ===================================
        public async Task<List<OrderModificationDto>> GetOrderModificationsAsync(long orderId)
        {
            try
            {
                string query = $@"
                    SELECT
                        m.c_modification_id, m.c_orderid, m.c_modification_type,
                        m.c_original_guest_count, m.c_modified_guest_count, m.c_additional_amount,
                        m.c_modification_reason, m.c_requested_by, m.c_approved_by,
                        m.c_status, m.c_payment_stage_id, m.c_created_date, m.c_approved_date,
                        o.c_order_number,
                        owner.c_business_name as requested_by_name,
                        u.c_name as approved_by_name
                    FROM {Table.SysOrderModifications} m
                    INNER JOIN {Table.SysOrders} o ON m.c_orderid = o.c_orderid
                    INNER JOIN {Table.SysCateringOwner} owner ON m.c_requested_by = owner.c_pkid
                    LEFT JOIN {Table.SysUser} u ON m.c_approved_by = u.c_pkid
                    WHERE m.c_orderid = @OrderId
                    ORDER BY m.c_created_date DESC
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@OrderId", orderId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);
                List<OrderModificationDto> modifications = new List<OrderModificationDto>();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        modifications.Add(MapOrderModificationDto(row));
                    }
                }

                return modifications;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving order modifications: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET MODIFICATION BY ID
        // ===================================
        public async Task<OrderModificationDto?> GetModificationByIdAsync(long modificationId)
        {
            try
            {
                string query = $@"
                    SELECT
                        m.c_modification_id, m.c_orderid, m.c_modification_type,
                        m.c_original_guest_count, m.c_modified_guest_count, m.c_additional_amount,
                        m.c_modification_reason, m.c_requested_by, m.c_approved_by,
                        m.c_status, m.c_payment_stage_id, m.c_created_date, m.c_approved_date,
                        o.c_order_number,
                        owner.c_business_name as requested_by_name,
                        u.c_name as approved_by_name
                    FROM {Table.SysOrderModifications} m
                    INNER JOIN {Table.SysOrders} o ON m.c_orderid = o.c_orderid
                    INNER JOIN {Table.SysCateringOwner} owner ON m.c_requested_by = owner.c_pkid
                    LEFT JOIN {Table.SysUser} u ON m.c_approved_by = u.c_pkid
                    WHERE m.c_modification_id = @ModificationId
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@ModificationId", modificationId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);

                if (dt.Rows.Count > 0)
                {
                    return MapOrderModificationDto(dt.Rows[0]);
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving modification: " + ex.Message, ex);
            }
        }

        // ===================================
        // APPROVE MODIFICATION
        // ===================================
        public async Task<bool> ApproveModificationAsync(long modificationId, long userId, long? paymentStageId = null)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysOrderModifications}
                    SET
                        c_status = 'Approved',
                        c_approved_by = @UserId,
                        c_approved_date = GETDATE(),
                        c_payment_stage_id = @PaymentStageId
                    WHERE c_modification_id = @ModificationId AND c_status = 'Pending'
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@ModificationId", modificationId),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@PaymentStageId", (object)paymentStageId ?? DBNull.Value)
                };

                int rowsAffected = await _db.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error approving modification: " + ex.Message, ex);
            }
        }

        // ===================================
        // REJECT MODIFICATION
        // ===================================
        public async Task<bool> RejectModificationAsync(long modificationId, long userId, string rejectionReason)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysOrderModifications}
                    SET
                        c_status = 'Rejected',
                        c_approved_by = @UserId,
                        c_approved_date = GETDATE(),
                        c_modification_reason = c_modification_reason + ' | Rejection Reason: ' + @RejectionReason
                    WHERE c_modification_id = @ModificationId AND c_status = 'Pending'
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@ModificationId", modificationId),
                    new SqlParameter("@UserId", userId),
                    new SqlParameter("@RejectionReason", rejectionReason)
                };

                int rowsAffected = await _db.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error rejecting modification: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE MODIFICATION TO PAID
        // ===================================
        public async Task<bool> UpdateModificationToPaidAsync(long modificationId)
        {
            try
            {
                string query = $@"
                    UPDATE {Table.SysOrderModifications}
                    SET c_status = 'Paid'
                    WHERE c_modification_id = @ModificationId AND c_status = 'Approved'
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@ModificationId", modificationId)
                };

                int rowsAffected = await _db.ExecuteNonQueryAsync(query, parameters);
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating modification to paid: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET APPROVED MODIFICATIONS FOR PAYMENT STAGE
        // ===================================
        public async Task<List<OrderModificationDto>> GetApprovedModificationsByPaymentStageAsync(long paymentStageId)
        {
            try
            {
                string query = $@"
                    SELECT
                        m.c_modification_id, m.c_orderid, m.c_modification_type,
                        m.c_original_guest_count, m.c_modified_guest_count, m.c_additional_amount,
                        m.c_modification_reason, m.c_requested_by, m.c_approved_by,
                        m.c_status, m.c_payment_stage_id, m.c_created_date, m.c_approved_date,
                        o.c_order_number,
                        owner.c_business_name as requested_by_name,
                        u.c_name as approved_by_name
                    FROM {Table.SysOrderModifications} m
                    INNER JOIN {Table.SysOrders} o ON m.c_orderid = o.c_orderid
                    INNER JOIN {Table.SysCateringOwner} owner ON m.c_requested_by = owner.c_pkid
                    LEFT JOIN {Table.SysUser} u ON m.c_approved_by = u.c_pkid
                    WHERE m.c_payment_stage_id = @PaymentStageId AND m.c_status = 'Approved'
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PaymentStageId", paymentStageId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);
                List<OrderModificationDto> modifications = new List<OrderModificationDto>();

                if (dt.Rows.Count > 0)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        modifications.Add(MapOrderModificationDto(row));
                    }
                }

                return modifications;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving approved modifications: " + ex.Message, ex);
            }
        }

        // ===================================
        // HELPER: MAP ORDER MODIFICATION DTO
        // ===================================
        private OrderModificationDto MapOrderModificationDto(DataRow row)
        {
            return new OrderModificationDto
            {
                ModificationId = Convert.ToInt64(row["c_modification_id"]),
                OrderId = Convert.ToInt64(row["c_orderid"]),
                OrderNumber = row["c_order_number"].ToString() ?? string.Empty,
                ModificationType = row["c_modification_type"].ToString() ?? string.Empty,
                OriginalGuestCount = row["c_original_guest_count"] != DBNull.Value ? Convert.ToInt32(row["c_original_guest_count"]) : null,
                ModifiedGuestCount = row["c_modified_guest_count"] != DBNull.Value ? Convert.ToInt32(row["c_modified_guest_count"]) : null,
                AdditionalAmount = Convert.ToDecimal(row["c_additional_amount"]),
                ModificationReason = row["c_modification_reason"].ToString() ?? string.Empty,
                RequestedBy = Convert.ToInt64(row["c_requested_by"]),
                RequestedByName = row["requested_by_name"].ToString() ?? string.Empty,
                ApprovedBy = row["c_approved_by"] != DBNull.Value ? Convert.ToInt64(row["c_approved_by"]) : null,
                ApprovedByName = row["approved_by_name"] != DBNull.Value ? row["approved_by_name"].ToString() : null,
                Status = row["c_status"].ToString() ?? string.Empty,
                PaymentStageId = row["c_payment_stage_id"] != DBNull.Value ? Convert.ToInt64(row["c_payment_stage_id"]) : null,
                CreatedDate = Convert.ToDateTime(row["c_created_date"]),
                ApprovedDate = row["c_approved_date"] != DBNull.Value ? Convert.ToDateTime(row["c_approved_date"]) : null
            };
        }
    }
}
