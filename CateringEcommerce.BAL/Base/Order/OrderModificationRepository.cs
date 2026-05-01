using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Order;
using CateringEcommerce.Domain.Models.Order;
using Npgsql;
using NpgsqlTypes;
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
                new NpgsqlParameter("@OrderId", request.OrderId),
                new NpgsqlParameter("@UserId", request.UserId),
                new NpgsqlParameter("@NewGuestCount", request.NewGuestCount),
                new NpgsqlParameter("@ChangeReason", request.ChangeReason),
                new NpgsqlParameter("@ModificationId", NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@PricingMultiplier", NpgsqlDbType.Double) { Direction = ParameterDirection.Output, Precision = 5, Scale = 2 },
                new NpgsqlParameter("@AdditionalCost", NpgsqlDbType.Double) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 },
                new NpgsqlParameter("@RequiresApproval", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output }
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
                new NpgsqlParameter("@OrderId", request.OrderId),
                new NpgsqlParameter("@UserId", request.UserId),
                new NpgsqlParameter("@NewMenuItems", request.MenuChanges),
                new NpgsqlParameter("@ChangeReason", request.ChangeReason),
                new NpgsqlParameter("@ModificationId", NpgsqlDbType.Bigint) { Direction = ParameterDirection.Output },
                new NpgsqlParameter("@AdditionalCost", NpgsqlDbType.Double) { Direction = ParameterDirection.Output, Precision = 18, Scale = 2 },
                new NpgsqlParameter("@RequiresApproval", NpgsqlDbType.Boolean) { Direction = ParameterDirection.Output }
            };

            var result = await _dbHelper.ExecuteStoredProcedureAsync<ModificationRequestResponse>(
                "sp_RequestMenuChange",
                parameters
            );

            return result;
        }

        public async Task<OrderModificationModel> GetModificationAsync(long modificationId)
        {
            var query = $@"
                SELECT * FROM {Table.SysOrderModifications}
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new NpgsqlParameter("@ModificationId", modificationId)
            };

            var results = await _dbHelper.ExecuteQueryAsync<OrderModificationModel>(query, parameters);
            return results.Count > 0 ? results[0] : null;
        }

        public async Task<List<OrderModificationModel>> GetOrderModificationsAsync(long orderId)
        {
            var query = $@"
                SELECT * FROM {Table.SysOrderModifications}
                WHERE c_orderid = @OrderId
                ORDER BY c_createddate DESC";

            var parameters = new[]
            {
                new NpgsqlParameter("@OrderId", orderId)
            };

            return await _dbHelper.ExecuteQueryAsync<OrderModificationModel>(query, parameters);
        }

        public async Task<List<OrderModificationModel>> GetPendingModificationsAsync()
        {
            var query = $@"
                SELECT om.*, o.c_ordernumber, o.c_event_date, u.c_username, u.c_email
                FROM {Table.SysOrderModifications} om
                INNER JOIN {Table.SysOrders} o ON om.c_orderid = o.c_orderid
                INNER JOIN {Table.SysUser} u ON om.c_userid = u.c_userid
                WHERE om.c_status = 'Pending'
                  AND om.c_requires_approval = 1
                ORDER BY om.c_createddate ASC";

            return await _dbHelper.ExecuteQueryAsync<OrderModificationModel>(query);
        }

        public async Task<List<OrderModificationModel>> GetPendingModificationsForPartnerAsync(long partnerId)
        {
            var query = $@"
                SELECT om.*, o.c_ordernumber, o.c_event_date, u.c_username, u.c_email
                FROM {Table.SysOrderModifications} om
                INNER JOIN {Table.SysOrders} o ON om.c_orderid = o.c_orderid
                INNER JOIN {Table.SysUser} u ON om.c_userid = u.c_userid
                WHERE o.c_cateringownerid = @PartnerId
                  AND om.c_status = 'Pending'
                  AND om.c_requires_approval = 1
                ORDER BY om.c_createddate ASC";

            var parameters = new[]
            {
                new NpgsqlParameter("@PartnerId", partnerId)
            };

            return await _dbHelper.ExecuteQueryAsync<OrderModificationModel>(query, parameters);
        }

        public async Task<bool> ApproveModificationAsync(long modificationId, long approvedBy, string approvedByType)
        {
            var query = $@"
                UPDATE {Table.SysOrderModifications}
                SET c_status = 'Approved',
                    c_admin_approved_by = @ApprovedBy,
                    c_admin_approval_date = NOW(),
                    c_admin_notes = 'Approved by ' || @ApprovedByType,
                    c_modifieddate = NOW()
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new NpgsqlParameter("@ModificationId", modificationId),
                new NpgsqlParameter("@ApprovedBy", approvedBy),
                new NpgsqlParameter("@ApprovedByType", approvedByType)
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
            var query = $@"
                UPDATE {Table.SysOrderModifications}
                SET c_status = 'Rejected',
                    c_admin_approved_by = @RejectedBy,
                    c_admin_approval_date = NOW(),
                    c_admin_notes = @RejectionReason,
                    c_modifieddate = NOW()
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new NpgsqlParameter("@ModificationId", modificationId),
                new NpgsqlParameter("@RejectedBy", rejectedBy),
                new NpgsqlParameter("@RejectionReason", rejectionReason)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> MarkModificationPaidAsync(long modificationId, long paymentTransactionId)
        {
            var query = $@"
                UPDATE {Table.SysOrderModifications}
                SET c_payment_status = 'Paid',
                    c_payment_transaction_id = @PaymentTransactionId,
                    c_payment_date = NOW(),
                    c_modifieddate = NOW()
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new NpgsqlParameter("@ModificationId", modificationId),
                new NpgsqlParameter("@PaymentTransactionId", paymentTransactionId)
            };

            var rowsAffected = await _dbHelper.ExecuteNonQueryAsync(query, parameters);
            return rowsAffected > 0;
        }

        public async Task<bool> AutoApproveModificationAsync(long modificationId)
        {
            var query = $@"
                UPDATE {Table.SysOrderModifications}
                SET c_status = 'Auto_Approved',
                    c_admin_approval_date = NOW(),
                    c_admin_notes = 'Auto-approved: Within allowed threshold',
                    c_modifieddate = NOW()
                WHERE c_modification_id = @ModificationId";

            var parameters = new[]
            {
                new NpgsqlParameter("@ModificationId", modificationId)
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
                var updateOrderQuery = $@"
                    UPDATE {Table.SysOrders}
                    SET c_guest_count = c_guest_count + @GuestCountChange,
                        c_total_amount = c_total_amount + @AdditionalAmount,
                        c_modifieddate = NOW()
                    WHERE c_orderid = @OrderId";

                var parameters = new[]
                {
                    new NpgsqlParameter("@OrderId", modification.OrderId),
                    new NpgsqlParameter("@GuestCountChange", modification.GuestCountChange),
                    new NpgsqlParameter("@AdditionalAmount", modification.AdditionalAmount)
                };

                await _dbHelper.ExecuteNonQueryAsync(updateOrderQuery, parameters);

                // Update payment summary
                var updatePaymentQuery = $@"
                    UPDATE {Table.SysPaymentSummary}
                    SET c_total_amount = c_total_amount + @AdditionalAmount,
                        c_remaining_balance = c_remaining_balance + @AdditionalAmount,
                        c_modifieddate = NOW()
                    WHERE c_orderid = @OrderId";

                await _dbHelper.ExecuteNonQueryAsync(updatePaymentQuery, parameters);
            }
            else if (modification.ModificationType == "MENU_CHANGE")
            {
                // Update order total
                var updateOrderQuery = $@"
                    UPDATE {Table.SysOrders}
                    SET c_total_amount = c_total_amount + @AdditionalAmount,
                        c_modifieddate = NOW()
                    WHERE c_orderid = @OrderId";

                var parameters = new[]
                {
                    new NpgsqlParameter("@OrderId", modification.OrderId),
                    new NpgsqlParameter("@AdditionalAmount", modification.AdditionalAmount)
                };

                await _dbHelper.ExecuteNonQueryAsync(updateOrderQuery, parameters);

                // Update payment summary
                var updatePaymentQuery = $@"
                    UPDATE {Table.SysPaymentSummary}
                    SET c_total_amount = c_total_amount + @AdditionalAmount,
                        c_remaining_balance = c_remaining_balance + @AdditionalAmount,
                        c_modifieddate = NOW()
                    WHERE c_orderid = @OrderId";

                await _dbHelper.ExecuteNonQueryAsync(updatePaymentQuery, parameters);
            }
        }

        #endregion
    }
}

