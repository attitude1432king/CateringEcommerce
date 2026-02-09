using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IOwnerCustomerRepository
    {
        /// <summary>
        /// Get filtered and paginated customers list
        /// </summary>
        Task<PaginatedCustomersDto> GetCustomersList(long ownerId, CustomerFilterDto filter);

        /// <summary>
        /// Get customer details with statistics
        /// </summary>
        Task<CustomerDetailDto> GetCustomerDetails(long ownerId, long customerId);

        /// <summary>
        /// Get customer order history
        /// </summary>
        Task<CustomerOrderHistoryDto> GetCustomerOrderHistory(long ownerId, long customerId);

        /// <summary>
        /// Get customer insights and analytics
        /// </summary>
        Task<CustomerInsightsDto> GetCustomerInsights(long ownerId);

        /// <summary>
        /// Get top customers by revenue or order count
        /// </summary>
        Task<List<TopCustomerDto>> GetTopCustomers(long ownerId, int limit = 10, string sortBy = "LifetimeValue");

        /// <summary>
        /// Validate if customer has ordered from owner
        /// </summary>
        Task<bool> ValidateCustomerOwnership(long ownerId, long customerId);
    }
}
