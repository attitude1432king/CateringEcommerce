using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface IOrderRepository
    {
        Task<long> InsertOrderAsync(long userId, CreateOrderDto orderData, string orderNumber);
        Task<bool> InsertOrderItemsAsync(long orderId, List<CreateOrderItemDto> items);
        Task<bool> InsertOrderPaymentAsync(long orderId, string paymentMethod, decimal amount, string? paymentProofPath = null, string? paymentStageType = "Full");
        Task<bool> InsertOrderStatusHistoryAsync(long orderId, string status, string? remarks = null, long? updatedBy = null);
        Task<string> GenerateOrderNumberAsync();
        Task<List<OrderListItemDto>> GetOrdersByUserIdAsync(long userId, int pageNumber = 1, int pageSize = 10);
        Task<OrderDto?> GetOrderByIdAsync(long orderId, long userId);
        Task<bool> UpdateOrderStatusAsync(long orderId, string status, string? remarks = null);
        Task<bool> CancelOrderAsync(long orderId, long userId, string reason);
        Task<bool> CheckCateringAvailabilityAsync(long cateringId, DateTime eventDate);
    }
}
