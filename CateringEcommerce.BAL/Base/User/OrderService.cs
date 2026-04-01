using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.BAL.Services;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Interfaces.User;
using CateringEcommerce.Domain.Models.User;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Base.User
{
    public class OrderService : IOrderService
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly IOrderRepository _orderRepository;
        private readonly IPaymentStageRepository _paymentStageRepository;
        private readonly INotificationHelper _notificationHelper;
        private readonly INotificationService _notificationService;
        private readonly IFileStorageService _fileStorageService;
        private readonly ISystemSettingsProvider _settingsProvider;
        private readonly ILogger<OrderService> _logger;

        public OrderService(
            IDatabaseHelper dbHelper,
            IOrderRepository orderRepository,
            IPaymentStageRepository paymentStageRepository,
            INotificationHelper notificationHelper,
            INotificationService notificationService,
            IFileStorageService fileStorageService,
            ISystemSettingsProvider settingsProvider,
            ILogger<OrderService> logger)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _paymentStageRepository = paymentStageRepository ?? throw new ArgumentNullException(nameof(paymentStageRepository));
            _notificationHelper = notificationHelper ?? throw new ArgumentNullException(nameof(notificationHelper));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            _fileStorageService = fileStorageService ?? throw new ArgumentNullException(nameof(fileStorageService));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ===================================
        // CREATE ORDER
        // ===================================
        public async Task<OrderDto> CreateOrderAsync(long userId, CreateOrderDto orderData)
        {
            try
            {
                // 1. Validate event date matches the platform's minimum advance booking rule
                int minAdvanceBookingDays = _settingsProvider.GetInt("BUSINESS.MIN_ADVANCE_BOOKING_DAYS", 5);
                DateTime minEventDate = DateTime.Today.AddDays(minAdvanceBookingDays);
                if (orderData.EventDate.Date < minEventDate)
                {
                    throw new InvalidOperationException($"Event date must be at least {minAdvanceBookingDays} days in advance.");
                }

                // 2. Check if catering is active and verified
                bool isCateringActive = await IsCateringActiveAsync(orderData.CateringId);
                if (!isCateringActive)
                {
                    throw new InvalidOperationException("This catering service is currently unavailable.");
                }

                // 3. Check catering availability for the event date using the same rules as the public availability API
                var availabilitySnapshot = await _orderRepository.GetCateringAvailabilitySnapshotAsync(orderData.CateringId, orderData.EventDate);
                var fallbackCapacity = _settingsProvider.GetInt("BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY", 1);
                var isAvailable =
                    availabilitySnapshot != null &&
                    availabilitySnapshot.Exists &&
                    availabilitySnapshot.IsApproved &&
                    availabilitySnapshot.IsActive &&
                    availabilitySnapshot.GlobalStatus != AvailabilityStatus.CLOSED &&
                    availabilitySnapshot.DateStatus != AvailabilityStatus.CLOSED &&
                    availabilitySnapshot.DateStatus != AvailabilityStatus.FULLY_BOOKED &&
                    availabilitySnapshot.ExistingBookingCount < (availabilitySnapshot.DailyBookingCapacity > 0
                        ? availabilitySnapshot.DailyBookingCapacity
                        : fallbackCapacity);

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
                    if (orderData.PaymentProof == null || orderData.PaymentProof.Length == 0)
                    {
                        throw new InvalidOperationException("Payment proof is required for bank transfer payment method.");
                    }

                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                    var validation = FileValidationHelper.ValidateFile(orderData.PaymentProof, allowedExtensions, 10 * 1024 * 1024);
                    if (!validation.IsValid)
                        throw new InvalidOperationException(validation.ErrorMessage);

                    var safeFilename = FileValidationHelper.GenerateSafeFilename(orderData.PaymentProof.FileName);
                    paymentProofPath = await _fileStorageService.SaveFormFileAsync(
                        orderData.PaymentProof,
                        orderData.CateringId,
                        DocumentType.PaymentProof.GetDisplayName(),
                        false,
                        safeFilename
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
                    if (_notificationHelper != null)
                    {
                        // Use new NotificationHelper
                        await _notificationHelper.SendOrderNotificationAsync(
                            "ORDER_CONFIRMATION",
                            order.ContactPerson ?? "Customer",
                            orderData.ContactEmail,
                            orderData.ContactPhone,
                            order.CateringName,
                            null, // Partner email - will be fetched if needed
                            null, // Partner phone - will be fetched if needed
                            new Dictionary<string, object>
                            {
                                { "customer_name", order.ContactPerson ?? "Customer" },
                                { "order_number", order.OrderNumber },
                                { "order_id", order.OrderId },
                                { "event_date", order.EventDate.ToString("dd MMM yyyy") },
                                { "event_time", order.EventTime },
                                { "event_location", order.EventLocation },
                                { "event_address", order.DeliveryAddress },
                                { "event_city", order.EventLocation }, // Using EventLocation as city
                                { "guest_count", order.GuestCount },
                                { "total_amount", order.TotalAmount.ToString("N2") },
                                { "payment_status", order.PaymentStatus ?? "Pending" },
                                { "catering_name", order.CateringName },
                                { "order_url", $"https://enyvora.com/orders/{order.OrderId}" },
                                { "support_email", "support@enyvora.com" },
                                { "support_phone", "+91-1234567890" }
                            },
                            notifyCustomer: true,
                            notifyPartner: false, // Partner will be notified when order is assigned
                            notifyAdmin: true
                        );
                    }
                    else
                    {
                        // Fallback to legacy notification service
                        await _notificationService.SendOrderConfirmationAsync(
                            order,
                            orderData.ContactEmail,
                            orderData.ContactPhone
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the order creation
                    _logger?.LogError(ex, "Failed to send order confirmation notification. OrderId: {OrderId}", order.OrderId);
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
                    if (_notificationHelper != null)
                    {
                        // Use new NotificationHelper
                        await _notificationHelper.SendOrderNotificationAsync(
                            "ORDER_CANCELLATION",
                            order.ContactPerson ?? "Customer",
                            order.ContactEmail,
                            order.ContactPhone,
                            order.CateringName,
                            null, // Partner email - will be fetched if needed
                            null, // Partner phone - will be fetched if needed
                            new Dictionary<string, object>
                            {
                                { "customer_name", order.ContactPerson ?? "Customer" },
                                { "order_number", order.OrderNumber },
                                { "order_id", order.OrderId },
                                { "event_date", order.EventDate.ToString("dd MMM yyyy") },
                                { "cancellation_reason", reason },
                                { "refund_amount", order.TotalAmount.ToString("N2") },
                                { "refund_timeline", "5-7 business days" },
                                { "catering_name", order.CateringName },
                                { "support_email", "support@enyvora.com" },
                                { "support_phone", "+91-1234567890" }
                            },
                            notifyCustomer: true,
                            notifyPartner: true, // Notify partner about cancellation
                            notifyAdmin: true
                        );
                    }
                    else
                    {
                        // Fallback to legacy notification service
                        await _notificationService.SendOrderCancellationAsync(
                            orderId,
                            order.OrderNumber,
                            order.ContactEmail,
                            order.ContactPhone,
                            reason
                        );
                    }
                }
                catch (Exception ex)
                {
                    // Log but don't fail the cancellation
                    _logger?.LogError(ex, "Failed to send order cancellation notification. OrderId: {OrderId}", orderId);
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

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
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
                      AND c_is_active = 1
                ";

                SqlParameter[] parameters = new SqlParameter[]
                {
                    new SqlParameter("@PackageId", packageId),
                    new SqlParameter("@CateringId", cateringId)
                };

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
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

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
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

                DataTable dt = await _dbHelper.ExecuteAsync(query, parameters);
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
