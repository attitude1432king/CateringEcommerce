using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Owner;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.BAL.Base.Owner
{
    public class OrderModificationService : IOrderModificationService
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly OrderModificationRepository _modificationRepository;
        private readonly PaymentStageRepository _paymentStageRepository;
        private readonly INotificationService? _notificationService;

        public OrderModificationService(IDatabaseHelper dbHelper, INotificationService? notificationService = null)
        {
            _dbHelper = dbHelper;
            _modificationRepository = new OrderModificationRepository(dbHelper);
            _paymentStageRepository = new PaymentStageRepository(dbHelper);
            _notificationService = notificationService;
        }

        // ===================================
        // CREATE ORDER MODIFICATION
        // ===================================
        public async Task<OrderModificationDto> CreateModificationAsync(CreateOrderModificationDto modificationData)
        {
            try
            {
                if (modificationData == null)
                {
                    throw new ArgumentNullException(nameof(modificationData));
                }

                // Validate modification data
                ValidateModificationData(modificationData);

                // Block modifications during live event
                string statusCheckQuery = $"SELECT c_order_status FROM {Table.SysOrders} WHERE c_orderid = @OrderId AND c_isactive = 1";
                var statusParams = new SqlParameter[] { new SqlParameter("@OrderId", modificationData.OrderId) };
                DataTable statusDt = await _dbHelper.ExecuteAsync(statusCheckQuery, statusParams);
                if (statusDt.Rows.Count > 0 && statusDt.Rows[0]["c_order_status"]?.ToString() == "InProgress")
                {
                    throw new InvalidOperationException("Order modifications are not allowed during a live event.");
                }

                // Insert modification
                long modificationId = await _modificationRepository.InsertOrderModificationAsync(modificationData);
                if (modificationId <= 0)
                {
                    throw new InvalidOperationException("Failed to create order modification. Please try again.");
                }

                // Retrieve created modification
                var modification = await _modificationRepository.GetModificationByIdAsync(modificationId);
                if (modification == null)
                {
                    throw new InvalidOperationException("Modification created but failed to retrieve details.");
                }

                // Send notification to user about the modification
                if (_notificationService != null)
                {
                    await SendModificationNotificationToUserAsync(modification);
                }

                return modification;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating order modification: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET ORDER MODIFICATIONS
        // ===================================
        public async Task<OrderModificationsSummaryDto> GetOrderModificationsAsync(long orderId)
        {
            try
            {
                if (orderId <= 0)
                {
                    throw new ArgumentException("Invalid order ID.", nameof(orderId));
                }

                var modifications = await _modificationRepository.GetOrderModificationsAsync(orderId);

                // Calculate summary
                decimal totalAdditionalAmount = modifications.Sum(m => m.AdditionalAmount);
                int totalModifications = modifications.Count;
                int pendingModifications = modifications.Count(m => m.Status == "Pending");
                int approvedModifications = modifications.Count(m => m.Status == "Approved");
                int rejectedModifications = modifications.Count(m => m.Status == "Rejected");
                int paidModifications = modifications.Count(m => m.Status == "Paid");

                string orderNumber = modifications.FirstOrDefault()?.OrderNumber ?? string.Empty;

                return new OrderModificationsSummaryDto
                {
                    OrderId = orderId,
                    OrderNumber = orderNumber,
                    TotalAdditionalAmount = totalAdditionalAmount,
                    TotalModifications = totalModifications,
                    PendingModifications = pendingModifications,
                    ApprovedModifications = approvedModifications,
                    RejectedModifications = rejectedModifications,
                    PaidModifications = paidModifications,
                    Modifications = modifications
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving order modifications: " + ex.Message, ex);
            }
        }

        // ===================================
        // APPROVE MODIFICATION
        // ===================================
        public async Task<OrderModificationDto> ApproveModificationAsync(ApproveOrderModificationDto approvalData)
        {
            try
            {
                if (approvalData == null)
                {
                    throw new ArgumentNullException(nameof(approvalData));
                }

                if (approvalData.ModificationId <= 0)
                {
                    throw new ArgumentException("Invalid modification ID.", nameof(approvalData.ModificationId));
                }

                if (approvalData.UserId <= 0)
                {
                    throw new ArgumentException("Invalid user ID.", nameof(approvalData.UserId));
                }

                // Get modification details
                var modification = await _modificationRepository.GetModificationByIdAsync(approvalData.ModificationId);
                if (modification == null)
                {
                    throw new InvalidOperationException("Modification not found.");
                }

                if (modification.Status != "Pending")
                {
                    throw new InvalidOperationException($"Cannot approve modification with status: {modification.Status}");
                }

                // Find the PostEvent payment stage for this order
                var paymentStages = await _paymentStageRepository.GetPaymentStagesByOrderIdAsync(modification.OrderId);
                var postEventStage = paymentStages.FirstOrDefault(ps => ps.StageType == "PostEvent");

                long? paymentStageId = postEventStage?.PaymentStageId;

                // If there's a PostEvent payment stage, we'll link the modification to it
                // The additional amount will be added to the PostEvent payment when the user pays

                // Approve modification
                bool approved = await _modificationRepository.ApproveModificationAsync(
                    approvalData.ModificationId,
                    approvalData.UserId,
                    paymentStageId
                );

                if (!approved)
                {
                    throw new InvalidOperationException("Failed to approve modification. Please try again.");
                }

                // Retrieve updated modification
                var updatedModification = await _modificationRepository.GetModificationByIdAsync(approvalData.ModificationId);
                if (updatedModification == null)
                {
                    throw new InvalidOperationException("Modification approved but failed to retrieve details.");
                }

                // Send notification to owner about approval
                if (_notificationService != null)
                {
                    await SendModificationApprovalNotificationToOwnerAsync(updatedModification);
                }

                return updatedModification;
            }
            catch (Exception ex)
            {
                throw new Exception("Error approving modification: " + ex.Message, ex);
            }
        }

        // ===================================
        // REJECT MODIFICATION
        // ===================================
        public async Task<OrderModificationDto> RejectModificationAsync(RejectOrderModificationDto rejectionData)
        {
            try
            {
                if (rejectionData == null)
                {
                    throw new ArgumentNullException(nameof(rejectionData));
                }

                if (rejectionData.ModificationId <= 0)
                {
                    throw new ArgumentException("Invalid modification ID.", nameof(rejectionData.ModificationId));
                }

                if (rejectionData.UserId <= 0)
                {
                    throw new ArgumentException("Invalid user ID.", nameof(rejectionData.UserId));
                }

                if (string.IsNullOrWhiteSpace(rejectionData.RejectionReason))
                {
                    throw new ArgumentException("Rejection reason is required.", nameof(rejectionData.RejectionReason));
                }

                // Get modification details
                var modification = await _modificationRepository.GetModificationByIdAsync(rejectionData.ModificationId);
                if (modification == null)
                {
                    throw new InvalidOperationException("Modification not found.");
                }

                if (modification.Status != "Pending")
                {
                    throw new InvalidOperationException($"Cannot reject modification with status: {modification.Status}");
                }

                // Reject modification
                bool rejected = await _modificationRepository.RejectModificationAsync(
                    rejectionData.ModificationId,
                    rejectionData.UserId,
                    rejectionData.RejectionReason
                );

                if (!rejected)
                {
                    throw new InvalidOperationException("Failed to reject modification. Please try again.");
                }

                // Retrieve updated modification
                var updatedModification = await _modificationRepository.GetModificationByIdAsync(rejectionData.ModificationId);
                if (updatedModification == null)
                {
                    throw new InvalidOperationException("Modification rejected but failed to retrieve details.");
                }

                // Send notification to owner about rejection
                if (_notificationService != null)
                {
                    await SendModificationRejectionNotificationToOwnerAsync(updatedModification, rejectionData.RejectionReason);
                }

                return updatedModification;
            }
            catch (Exception ex)
            {
                throw new Exception("Error rejecting modification: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET TOTAL MODIFICATION AMOUNT FOR POST-EVENT PAYMENT
        // ===================================
        public async Task<decimal> GetTotalModificationAmountForPaymentStageAsync(long paymentStageId)
        {
            try
            {
                var modifications = await _modificationRepository.GetApprovedModificationsByPaymentStageAsync(paymentStageId);
                return modifications.Sum(m => m.AdditionalAmount);
            }
            catch (Exception ex)
            {
                throw new Exception("Error calculating modification amount: " + ex.Message, ex);
            }
        }

        // ===================================
        // MARK MODIFICATIONS AS PAID
        // ===================================
        public async Task MarkModificationsAsPaidAsync(long paymentStageId)
        {
            try
            {
                var modifications = await _modificationRepository.GetApprovedModificationsByPaymentStageAsync(paymentStageId);

                foreach (var modification in modifications)
                {
                    await _modificationRepository.UpdateModificationToPaidAsync(modification.ModificationId);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error marking modifications as paid: " + ex.Message, ex);
            }
        }

        // ===================================
        // VALIDATE MODIFICATION DATA
        // ===================================
        private void ValidateModificationData(CreateOrderModificationDto modificationData)
        {
            if (modificationData.OrderId <= 0)
            {
                throw new ArgumentException("Invalid order ID.", nameof(modificationData.OrderId));
            }

            if (string.IsNullOrWhiteSpace(modificationData.ModificationType))
            {
                throw new ArgumentException("Modification type is required.", nameof(modificationData.ModificationType));
            }

            var validTypes = new[] { "GuestCountIncrease", "ItemAddition", "ServiceExtension", "DecorationUpgrade" };
            if (!Array.Exists(validTypes, type => type.Equals(modificationData.ModificationType, StringComparison.OrdinalIgnoreCase)))
            {
                throw new ArgumentException("Invalid modification type.", nameof(modificationData.ModificationType));
            }

            if (modificationData.AdditionalAmount <= 0)
            {
                throw new ArgumentException("Additional amount must be greater than zero.", nameof(modificationData.AdditionalAmount));
            }

            if (string.IsNullOrWhiteSpace(modificationData.ModificationReason))
            {
                throw new ArgumentException("Modification reason is required.", nameof(modificationData.ModificationReason));
            }

            if (modificationData.RequestedBy <= 0)
            {
                throw new ArgumentException("Requested by (Owner ID) is required.", nameof(modificationData.RequestedBy));
            }

            // Validate guest count for GuestCountIncrease type
            if (modificationData.ModificationType == "GuestCountIncrease")
            {
                if (!modificationData.OriginalGuestCount.HasValue || !modificationData.ModifiedGuestCount.HasValue)
                {
                    throw new ArgumentException("Original and modified guest counts are required for guest count increase.", nameof(modificationData.OriginalGuestCount));
                }

                if (modificationData.ModifiedGuestCount <= modificationData.OriginalGuestCount)
                {
                    throw new ArgumentException("Modified guest count must be greater than original guest count.", nameof(modificationData.ModifiedGuestCount));
                }
            }
        }

        // ===================================
        // SEND MODIFICATION NOTIFICATION TO USER
        // ===================================
        private async Task SendModificationNotificationToUserAsync(OrderModificationDto modification)
        {
            try
            {
                // This would require fetching user email/phone from order
                // For now, this is a placeholder
                string emailSubject = $"Order Modification Request - {modification.OrderNumber}";
                string emailBody = $@"
                    <h2>Order Modification Request</h2>
                    <p>A modification has been requested for your order <strong>{modification.OrderNumber}</strong>.</p>
                    <p><strong>Modification Type:</strong> {modification.ModificationType}</p>
                    <p><strong>Additional Amount:</strong> ₹{modification.AdditionalAmount:N2}</p>
                    <p><strong>Reason:</strong> {modification.ModificationReason}</p>
                    <p>Please log in to your account to review and approve/reject this modification.</p>
                ";

                // await _notificationService.SendEmailAsync(userEmail, emailSubject, emailBody);
            }
            catch
            {
                // Log error but don't throw - notification failure shouldn't break the flow
            }
        }

        // ===================================
        // SEND APPROVAL NOTIFICATION TO OWNER
        // ===================================
        private async Task SendModificationApprovalNotificationToOwnerAsync(OrderModificationDto modification)
        {
            try
            {
                // Placeholder for owner notification
                string message = $"Order modification #{modification.ModificationId} has been approved by the customer.";
                // await _notificationService.SendEmailAsync(ownerEmail, "Modification Approved", message);
            }
            catch
            {
                // Log error but don't throw
            }
        }

        // ===================================
        // SEND REJECTION NOTIFICATION TO OWNER
        // ===================================
        private async Task SendModificationRejectionNotificationToOwnerAsync(OrderModificationDto modification, string reason)
        {
            try
            {
                // Placeholder for owner notification
                string message = $"Order modification #{modification.ModificationId} has been rejected. Reason: {reason}";
                // await _notificationService.SendEmailAsync(ownerEmail, "Modification Rejected", message);
            }
            catch
            {
                // Log error but don't throw
            }
        }
    }
}
