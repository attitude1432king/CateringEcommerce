using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Order;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Order
{
    public class OrderModificationRepository : IOrderModificationRepository
    {
        private readonly IDatabaseHelper _dbHelper;

        public OrderModificationRepository(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<ModificationRequestResponse> RequestGuestCountChangeAsync(GuestCountChangeRequestDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@OrderId", request.OrderId),
                new SqlParameter("@UserId", request.UserId),
                new SqlParameter("@NewGuestCount", request.NewGuestCount),
                new SqlParameter("@ChangeReason", request.ChangeReason),
                new SqlParameter("@ModificationId", SqlDbType.BigInt) { Direction = ParameterDirection.Output },
                new SqlParameter("@PricingMultiplier", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 },
                new SqlParameter("@AdditionalCost", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 },
                new SqlParameter("@RequiresApproval", SqlDbType.Bit) { Direction = ParameterDirection.Output }
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<ModificationRequestResponse>(
                "sp_RequestGuestCountChange",
                parameters
            );

            return result;
        }

        public async Task<ModificationRequestResponse> RequestMenuChangeAsync(MenuChangeRequestDto request)
        {
            var parameters = new[]
            {
                new SqlParameter("@OrderId", request.OrderId),
                new SqlParameter("@UserId", request.UserId),
                new SqlParameter("@NewMenuItems", request.MenuChanges),
                new SqlParameter("@ChangeReason", request.ChangeReason),
                new SqlParameter("@ModificationId", SqlDbType.BigInt) { Direction = ParameterDirection.Output },
                new SqlParameter("@AdditionalCost", SqlDbType.Decimal) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 },
                new SqlParameter("@RequiresApproval", SqlDbType.Bit) { Direction = ParameterDirection.Output }
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<ModificationRequestResponse>(
                "sp_RequestMenuChange",
                parameters
            );

            return result;
        }

        public async Task<OrderModificationModel> GetModificationAsync(long modificationId)
        {
            var query = @"
                SELECT * FROM t_sys_order_modifications
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new SqlParameter("@ModificationId", modificationId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<OrderModificationModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<OrderModificationModel>> GetOrderModificationsAsync(long orderId)
        {
            var query = @"
                SELECT * FROM t_sys_order_modifications
                WHERE c_orderid = @OrderId
                ORDER BY c_created_date DESC";

            var parameters = new[]
            {
                new SqlParameter("@OrderId", orderId)
            };

            return await _dbHelper.ExecuteQueryAsync<OrderModificationModel>(query, parameters);
        }

        public async Task<List<OrderModificationModel>> GetPendingModificationsAsync()
        {
            var query = @"
                SELECT om.*, o.c_ordernumber, o.c_event_date, u.c_username, u.c_email
                FROM t_sys_order_modifications om
                INNER JOIN t_sys_order o ON om.c_orderid = o.c_orderid
                INNER JOIN t_sys_user u ON om.c_userid = u.c_userid
                WHERE om.c_status = 'Pending'
                  AND om.c_requires_approval = 1
                ORDER BY om.c_created_date ASC";

            return await _dbHelper.ExecuteQueryAsync<OrderModificationModel>(query);
        }

        public async Task<List<OrderModificationModel>> GetPendingModificationsForPartnerAsync(long partnerId)
        {
            var query = @"
                SELECT om.*, o.c_ordernumber, o.c_event_date, u.c_username, u.c_email
                FROM t_sys_order_modifications om
                INNER JOIN t_sys_order o ON om.c_orderid = o.c_orderid
                INNER JOIN t_sys_user u ON om.c_userid = u.c_userid
                WHERE o.c_cateringownerid = @PartnerId
                  AND om.c_status = 'Pending'
                  AND om.c_requires_approval = 1
                ORDER BY om.c_created_date ASC";

            var parameters = new[]
            {
                new SqlParameter("@PartnerId", partnerId)
            };

            return await _dbHelper.ExecuteQueryAsync<OrderModificationModel>(query, parameters);
        }

        public async Task<bool> ApproveModificationAsync(long modificationId, long approvedBy, string approvedByType)
        {
            var query = @"
                UPDATE t_sys_order_modifications
                SET c_status = 'Approved',
                    c_admin_approved_by = @ApprovedBy,
                    c_admin_approval_date = GETDATE(),
                    c_admin_notes = 'Approved by ' + @ApprovedByType,
                    c_modified_date = GETDATE()
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new SqlParameter("@ModificationId", modificationId),
                new SqlParameter("@ApprovedBy", approvedBy),
                new SqlParameter("@ApprovedByType", approvedByType)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);

            if (rowsAffected > 0)
            {
                // Apply the modification to the order
                await ApplyModificationToOrderAsync(modificationId);
            }

            return rowsAffected > 0;
        }

        public async Task<bool> RejectModificationAsync(long modificationId, long rejectedBy, string rejectionReason)
        {
            var query = @"
                UPDATE t_sys_order_modifications
                SET c_status = 'Rejected',
                    c_admin_approved_by = @RejectedBy,
                    c_admin_approval_date = GETDATE(),
                    c_admin_notes = @RejectionReason,
                    c_modified_date = GETDATE()
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new SqlParameter("@ModificationId", modificationId),
                new SqlParameter("@RejectedBy", rejectedBy),
                new SqlParameter("@RejectionReason", rejectionReason)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> MarkModificationPaidAsync(long modificationId, long paymentTransactionId)
        {
            var query = @"
                UPDATE t_sys_order_modifications
                SET c_payment_status = 'Paid',
                    c_payment_transaction_id = @PaymentTransactionId,
                    c_payment_date = GETDATE(),
                    c_modified_date = GETDATE()
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new SqlParameter("@ModificationId", modificationId),
                new SqlParameter("@PaymentTransactionId", paymentTransactionId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> AutoApproveModificationAsync(long modificationId)
        {
            var query = @"
                UPDATE t_sys_order_modifications
                SET c_status = 'Auto_Approved',
                    c_admin_approval_date = GETDATE(),
                    c_admin_notes = 'Auto-approved: Within allowed threshold',
                    c_modified_date = GETDATE()
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new SqlParameter("@ModificationId", modificationId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);

            if (rowsAffected > 0)
            {
                // Apply the modification to the order
                await ApplyModificationToOrderAsync(modificationId);
            }

            return rowsAffected > 0;
        }

        #region Helper Methods

        private async Task ApplyModificationToOrderAsync(long modificationId)
        {
            // Get modification details
            var modification = await GetModificationAsync(modificationId);
            if (modification == null) return;

            if (modification.ModificationType == "GUEST_COUNT_INCREASE" || modification.ModificationType == "GUEST_COUNT_DECREASE")
            {
                // Update order guest count
                var updateOrderQuery = @"
                    UPDATE t_sys_order
                    SET c_guest_count = c_guest_count + @GuestCountChange,
                        c_total_amount = c_total_amount + @AdditionalAmount,
                        c_modifieddate = GETDATE()
                    WHERE c_orderid = @OrderId";

                var parameters = new[]
                {
                    new SqlParameter("@OrderId", modification.OrderId),
                    new SqlParameter("@GuestCountChange", modification.GuestCountChange),
                    new SqlParameter("@AdditionalAmount", modification.AdditionalAmount)
                };

                await _dbHelper.ExecuteNonQueryAsync(updateOrderQuery, parameters);

                // Update payment summary
                var updatePaymentQuery = @"
                    UPDATE t_sys_payment_summary
                    SET c_total_amount = c_total_amount + @AdditionalAmount,
                        c_remaining_balance = c_remaining_balance + @AdditionalAmount,
                        c_modified_date = GETDATE()
                    WHERE c_orderid = @OrderId";

                await _dbHelper.ExecuteNonQueryAsync(updatePaymentQuery, parameters);
            }
            else if (modification.ModificationType == "MENU_CHANGE")
            {
                // Update order total
                var updateOrderQuery = @"
                    UPDATE t_sys_order
                    SET c_total_amount = c_total_amount + @AdditionalAmount,
                        c_modifieddate = GETDATE()
                    WHERE c_orderid = @OrderId";

                var parameters = new[]
                {
                    new SqlParameter("@OrderId", modification.OrderId),
                    new SqlParameter("@AdditionalAmount", modification.AdditionalAmount)
                };

                await _dbHelper.ExecuteNonQueryAsync(updateOrderQuery, parameters);

                // Update payment summary
                var updatePaymentQuery = @"
                    UPDATE t_sys_payment_summary
                    SET c_total_amount = c_total_amount + @AdditionalAmount,
                        c_remaining_balance = c_remaining_balance + @AdditionalAmount,
                        c_modified_date = GETDATE()
                    WHERE c_orderid = @OrderId";

                await _dbHelper.ExecuteNonQueryAsync(updatePaymentQuery, parameters);
            }
        }

        #endregion
    }
}
