using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Extensions;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;

namespace CateringEcommerce.BAL.Base.User
{
    public class OrderService
    {
        private readonly string _connectionString;
        private readonly OrderRepository _orderRepository;
        private readonly PaymentStageRepository _paymentStageRepository;
        private readonly INotificationService _notificationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly SqlDatabaseManager _db;

        public OrderService(string connectionString, INotificationService notificationService, IFileStorageService fileStorageService)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _orderRepository = new OrderRepository(connectionString);
            _paymentStageRepository = new PaymentStageRepository(connectionString);
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        // ===================================
        // CREATE ORDER
        // ===================================
        public async Task<OrderDto> CreateOrderAsync(long userId, CreateOrderDto orderData)
        {
            try
            {
                // 1. Validate event date is at least 24 hours in advance
                DateTime minEventDate = DateTime.Now.AddHours(24);
                if (orderData.EventDate < minEventDate)
                {
                    throw new InvalidOperationException("Event date must be at least 24 hours in advance.");
                }

                // 2. Check if catering is active and verified
                bool isCateringActive = await IsCateringActiveAsync(orderData.CateringId);
                if (!isCateringActive)
                {
                    throw new InvalidOperationException("This catering service is currently unavailable.");
                }

                // 3. Check catering availability for the event date
                bool isAvailable = await _orderRepository.CheckCateringAvailabilityAsync(orderData.CateringId, orderData.EventDate);
                if (!isAvailable)
                {
                    throw new InvalidOperationException("Sorry, this catering is not available on your selected date. Please choose another date.");
                }

                // 4. Validate cart items still exist and are active
                bool itemsValid = await ValidateOrderItemsAsync(orderData.OrderItems, orderData.CateringId);
                if (!itemsValid)
                {
                    throw new InvalidOperationException("Some items in your cart are no longer available.");
                }

                // 5. For Bank Transfer: Handle payment proof upload
                string? paymentProofPath = null;
                if (orderData.PaymentMethod == "BankTransfer")
                {
                    if (orderData.PaymentProof == null || string.IsNullOrEmpty(orderData.PaymentProof.Base64))
                    {
                        throw new InvalidOperationException("Payment proof is required for bank transfer payment method.");
                    }

                    // Upload payment proof
                    paymentProofPath = await _fileStorageService.SaveFileAsync(
                        orderData.PaymentProof.Base64,
                        orderData.CateringId, // Using catering ID for folder organization
                        DocumentType.PaymentProof.GetDisplayName(),
                        false, // Not secure
                        orderData.PaymentProof.Name
                    );
                }

                // 6. Generate unique order number
                string orderNumber = await _orderRepository.GenerateOrderNumberAsync();

                // 7. Insert order
                long orderId = await _orderRepository.InsertOrderAsync(userId, orderData, orderNumber);
                if (orderId <= 0)
                {
                    throw new InvalidOperationException("Failed to create order. Please try again.");
                }

                // 8. Insert order items
                await _orderRepository.InsertOrderItemsAsync(orderId, orderData.OrderItems);

                // 9. Handle Payment: Split or Full Payment
                if (orderData.EnableSplitPayment)
                {
                    // SPLIT PAYMENT: 40% pre-booking, 60% post-event
                    decimal preBookingAmount = orderData.PreBookingAmount ?? (orderData.TotalAmount * 0.40m);
                    decimal postEventAmount = orderData.PostEventAmount ?? (orderData.TotalAmount * 0.60m);

                    // Create pre-booking payment stage (due immediately)
                    await _paymentStageRepository.InsertPaymentStageAsync(
                        orderId,
                        "PreBooking",
                        40.00m,
                        preBookingAmount,
                        null // No due date for pre-booking
                    );

                    // Create post-event payment stage (due 1 day after event)
                    DateTime postEventDueDate = orderData.EventDate.AddDays(1);
                    await _paymentStageRepository.InsertPaymentStageAsync(
                        orderId,
                        "PostEvent",
                        60.00m,
                        postEventAmount,
                        postEventDueDate
                    );

                    // Insert payment record with split payment flag
                    await _orderRepository.InsertOrderPaymentAsync(
                        orderId,
                        orderData.PaymentMethod,
                        orderData.TotalAmount,
                        paymentProofPath,
                        "PreBooking" // Payment stage type for split payment
                    );
                }
                else
                {
                    // FULL PAYMENT: Traditional flow
                    await _orderRepository.InsertOrderPaymentAsync(
                        orderId,
                        orderData.PaymentMethod,
                        orderData.TotalAmount,
                        paymentProofPath,
                        "Full" // Payment stage type for full payment
                    );
                }

                // 10. Insert initial status history
                await _orderRepository.InsertOrderStatusHistoryAsync(
                    orderId,
                    "Pending",
                    "Order placed successfully"
                );

                // 11. Get complete order details
                OrderDto? order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
                if (order == null)
                {
                    throw new InvalidOperationException("Order created but failed to retrieve details.");
                }

                // 12. Send email and SMS notifications
                try
                {
                    await _notificationService.SendOrderConfirmationAsync(
                        order,
                        orderData.ContactEmail,
                        orderData.ContactPhone
                    );
                }
                catch (Exception ex)
                {
                    // Log but don't fail the order creation
                    Console.WriteLine($"Notification error: {ex.Message}");
                }

                return order;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error creating order: {ex.Message}", ex);
            }
        }

        // ===================================
        // GET USER ORDERS (PAGINATED)
        // ===================================
        public async Task<List<OrderListItemDto>> GetUserOrdersAsync(long userId, int pageNumber = 1, int pageSize = 10)
        {
            try
            {
                return await _orderRepository.GetOrdersByUserIdAsync(userId, pageNumber, pageSize);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching user orders.", ex);
            }
        }

        // ===================================
        // GET ORDER DETAILS
        // ===================================
        public async Task<OrderDto?> GetOrderDetailsAsync(long userId, long orderId)
        {
            try
            {
                return await _orderRepository.GetOrderByIdAsync(orderId, userId);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error fetching order details.", ex);
            }
        }

        // ===================================
        // CANCEL ORDER
        // ===================================
        public async Task<bool> CancelOrderAsync(long userId, long orderId, string reason)
        {
            try
            {
                // Get order details first
                OrderDto? order = await _orderRepository.GetOrderByIdAsync(orderId, userId);
                if (order == null)
                {
                    throw new InvalidOperationException("Order not found.");
                }

                // Cancel order (repository handles validation)
                await _orderRepository.CancelOrderAsync(orderId, userId, reason);

                // Send cancellation notification
                try
                {
                    await _notificationService.SendOrderCancellationAsync(
                        orderId,
                        order.OrderNumber,
                        order.ContactEmail,
                        order.ContactPhone,
                        reason
                    );
                }
                catch (Exception ex)
                {
                    // Log but don't fail the cancellation
                    Console.WriteLine($"Notification error: {ex.Message}");
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error cancelling order: {ex.Message}", ex);
            }
        }

        // ===================================
        // HELPER: CHECK IF CATERING IS ACTIVE
        // ===================================
        private async Task<bool> IsCateringActiveAsync(long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT c_isactive, c_verified_by_admin
                    FROM {Table.SysCateringOwner}
                    WHERE c_ownerid = @CateringId
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@CateringId", cateringId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);
                if (dt.Rows.Count == 0)
                    return false;

                bool isActive = Convert.ToBoolean(dt.Rows[0]["c_isactive"]);
                bool isVerified = Convert.ToBoolean(dt.Rows[0]["c_verified_by_admin"]);

                return isActive && isVerified;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error checking catering status.", ex);
            }
        }

        // ===================================
        // HELPER: VALIDATE ORDER ITEMS
        // ===================================
        private async Task<bool> ValidateOrderItemsAsync(List<CreateOrderItemDto> items, long cateringId)
        {
            try
            {
                foreach (var item in items)
                {
                    bool isValid = false;

                    if (item.ItemType == "Package")
                    {
                        isValid = await IsPackageValidAsync(item.ItemId, cateringId);
                    }
                    else if (item.ItemType == "FoodItem")
                    {
                        isValid = await IsFoodItemValidAsync(item.ItemId, cateringId);
                    }
                    else if (item.ItemType == "Decoration")
                    {
                        isValid = await IsDecorationValidAsync(item.ItemId, cateringId);
                    }

                    if (!isValid)
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error validating order items.", ex);
            }
        }

        // ===================================
        // HELPER: VALIDATE PACKAGE
        // ===================================
        private async Task<bool> IsPackageValidAsync(long packageId, long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT COUNT(*)
                    FROM {Table.SysMenuPackage}
                    WHERE c_packageid = @PackageId
                      AND c_ownerid = @CateringId
                      AND c_isactive = 1
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackageId", packageId),
                    new SqlParameter("@CateringId", cateringId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);
                int count = dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;

                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating package: {ex.Message}");
                return false;
            }
        }

        // ===================================
        // HELPER: VALIDATE FOOD ITEM
        // ===================================
        private async Task<bool> IsFoodItemValidAsync(long foodItemId, long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT COUNT(*)
                    FROM {Table.SysFoodItems}
                    WHERE c_foodid = @FoodItemId
                      AND c_ownerid = @CateringId
                      AND c_isactive = 1
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@FoodItemId", foodItemId),
                    new SqlParameter("@CateringId", cateringId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);
                int count = dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;

                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating food item: {ex.Message}");
                return false;
            }
        }

        // ===================================
        // HELPER: VALIDATE DECORATION
        // ===================================
        private async Task<bool> IsDecorationValidAsync(long decorationId, long cateringId)
        {
            try
            {
                string query = $@"
                    SELECT COUNT(*)
                    FROM {Table.SysCateringDecorations}
                    WHERE c_decorationid = @DecorationId
                      AND c_ownerid = @CateringId
                      AND c_isactive = 1
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@DecorationId", decorationId),
                    new SqlParameter("@CateringId", cateringId)
                };

                DataTable dt = await _db.ExecuteAsync(query, parameters);
                int count = dt.Rows.Count > 0 ? Convert.ToInt32(dt.Rows[0][0]) : 0;

                return count > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error validating decoration: {ex.Message}");
                return false;
            }
        }
    }
}
