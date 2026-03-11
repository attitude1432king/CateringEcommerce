using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces.User
{
    /// <summary>
    /// Order service for user-side order operations
    /// </summary>
    public interface IOrderService
    {
        /// <summary>
        /// Create a new order
        /// </summary>
        Task<OrderDto> CreateOrderAsync(long userId, CreateOrderDto orderData);

        /// <summary>
        /// Get user's orders with pagination
        /// </summary>
        Task<List<OrderListItemDto>> GetUserOrdersAsync(long userId, int pageNumber = 1, int pageSize = 10);

        /// <summary>
        /// Get detailed information for a specific order
        /// </summary>
        Task<OrderDto?> GetOrderDetailsAsync(long userId, long orderId);

        /// <summary>
        /// Cancel an order
        /// </summary>
        Task<bool> CancelOrderAsync(long userId, long orderId, string reason);
    }
}
