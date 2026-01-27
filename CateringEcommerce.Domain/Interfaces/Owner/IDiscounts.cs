using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IDiscounts
    {
        Task<int> AddDiscountAsync(long ownerPKID, DiscountDto discount);
        Task<int> UpdateDiscountAsync(long ownerPKID, DiscountDto discount);
        Task<bool> SoftDeleteDiscountAsync(long ownerPKID, long discountPKID);
        Task<List<DiscountModel>> GetDiscountListAsync(long ownerPKID, int page, int pageSize, string filterJson);
        Task<int> GetDiscountsCountAsync(long ownerPKID, string filterJson);
        Task<bool> IsValidDiscountId(long ownerPKID, long discountPKID);
        Task<bool> IsDiscountNameExists(long ownerPKID, string discountName, long? discountPKID = null);
        Task<bool> IsHigherThanSelectedItemPrice(string tableName, string pkColumnName, long ownerPKID, decimal price, List<long> selectedItemIds);
    }

}
