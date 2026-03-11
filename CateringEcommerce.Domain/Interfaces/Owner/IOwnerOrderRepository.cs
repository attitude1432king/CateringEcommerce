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
        /// <summary>
        /// Retrieves aggregated statistics for booking requests associated with the specified owner.
        /// </summary>
        /// <param name="ownerId">The unique identifier of the owner whose booking request statistics are to be retrieved.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains a <see
        /// cref="BookingRequestStatsDto"/> object with the aggregated booking request statistics for the specified
        /// owner.</returns>
        Task<BookingRequestStatsDto> GetBookingRequestStats(long ownerId);
    }
}
