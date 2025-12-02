using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IFoodItems
    {
        Task<Int32> GetFoodItemsCount(long ownerPKID, FoodItemFilter filter);
        Task<List<FoodItemModel>> GetFoodItems(long ownerPKID, int page, int pageSize, FoodItemFilter filter);
        Task<long> AddFoodItem(long ownerPKID, FoodItemDto foodItem);
        Task<int> DeleteFoodItem(long ownerPKID, long foodItemPKID);
        Task<int> UpdateFoodItem(long ownerPKID, FoodItemDto foodItem);
        Task<bool> IsFoodItemNameExists(long ownerPKID, string foodItemName, long? foodItemPKID = null);
    }
}
