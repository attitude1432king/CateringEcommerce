using CateringEcommerce.Domain.Models.User;
using System.Data;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface IPaymentStageRepository
    {
        Task<long> InsertPaymentStageAsync(long orderId, string stageType, decimal stagePercentage, decimal stageAmount, DateTime? dueDate = null);
        Task<List<PaymentStageDto>> GetPaymentStagesByOrderIdAsync(long orderId);
        Task<List<PaymentStageDto>> GetPendingPaymentStagesAsync(long orderId);
        Task<bool> UpdatePaymentStageStatusAsync(long paymentStageId, string status, ProcessPaymentStageDto paymentData);
        Task<bool> UpdatePaymentStageWithOrderStatusAsync(long paymentStageId, long orderId, string stageType, string status, ProcessPaymentStageDto paymentData, string newOrderStatus);
        Task<DataTable> GetOrdersWithPendingPostEventPaymentsAsync();
        Task<bool> UpdateReminderSentCountAsync(long paymentStageId);
    }
}
