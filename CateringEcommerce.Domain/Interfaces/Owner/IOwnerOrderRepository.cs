using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IOwnerOrderRepository
    {
        /// <summary>
        /// Get filtered and paginated orders list
        /// </summary>
        Task<PaginatedOrdersDto> GetOrdersList(long ownerId, OrderFilterDto filter);

        /// <summary>
        /// Get complete order details
        /// </summary>
        Task<OrderDetailDto> GetOrderDetails(long ownerId, long orderId);

        /// <summary>
        /// Update order status
        /// </summary>
        Task<bool> UpdateOrderStatus(long ownerId, long orderId, OrderStatusUpdateDto statusUpdate);

        /// <summary>
        /// Get order status history timeline
        /// </summary>
        Task<List<OrderStatusHistoryDto>> GetOrderStatusHistory(long orderId);

        /// <summary>
        /// Get order statistics
        /// </summary>
        Task<OrderStatsDto> GetOrderStats(long ownerId);

        /// <summary>
        /// Validate if order belongs to owner
        /// </summary>
        Task<bool> ValidateOrderOwnership(long ownerId, long orderId);
    }
}
