using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IAdminOrderRepository
    {
        /// <summary>
        /// Get paginated list of orders with filtering
        /// </summary>
        Task<AdminOrderListResponse> GetOrdersAsync(AdminOrderListRequest request);

        /// <summary>
        /// Get order details by ID
        /// </summary>
        Task<AdminOrderDetail?> GetOrderByIdAsync(long orderId);

        /// <summary>
        /// Update order status
        /// </summary>
        Task<bool> UpdateOrderStatusAsync(AdminOrderUpdateStatusRequest request);

        /// <summary>
        /// Get order statistics for dashboard
        /// </summary>
        Task<AdminOrderStatsResponse> GetOrderStatsAsync();

        /// <summary>
        /// Cancel order (admin initiated)
        /// </summary>
        Task<bool> CancelOrderAsync(long orderId, long adminId, string reason);

        /// <summary>
        /// Export orders to CSV/Excel
        /// </summary>
        Task<byte[]> ExportOrdersAsync(AdminOrderListRequest request);
    }
}
