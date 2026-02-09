using System.Collections.Generic;
using System.Threading.Tasks;
using CateringEcommerce.Domain.Models.Admin;

namespace CateringEcommerce.Domain.Interfaces.Admin
{
    public interface IMasterDataRepository
    {
        // ===== City Management =====
        Task<MasterDataListResponse<CityMasterItem>> GetCitiesAsync(MasterDataListRequest request);
        Task<long> CreateCityAsync(CreateMasterDataRequest request, long createdBy);
        Task<bool> UpdateCityAsync(UpdateMasterDataRequest request, long updatedBy);
        Task<bool> UpdateCityStatusAsync(long id, bool isActive, long updatedBy);
        Task<List<StateDropdownItem>> GetStatesAsync();

        // ===== Food Category Management =====
        Task<MasterDataListResponse<FoodCategoryMasterItem>> GetFoodCategoriesAsync(MasterDataListRequest request);
        Task<long> CreateFoodCategoryAsync(CreateMasterDataRequest request, long createdBy);
        Task<bool> UpdateFoodCategoryAsync(UpdateMasterDataRequest request, long updatedBy);
        Task<bool> UpdateFoodCategoryStatusAsync(long id, bool isActive, long updatedBy);

        // ===== Catering Type Management (Food, Cuisine, Event, Service) =====
        Task<MasterDataListResponse<CateringTypeMasterItem>> GetCateringTypesAsync(int categoryId, MasterDataListRequest request);
        Task<long> CreateCateringTypeAsync(CreateMasterDataRequest request, long createdBy);
        Task<bool> UpdateCateringTypeAsync(UpdateMasterDataRequest request, long updatedBy);
        Task<bool> UpdateCateringTypeStatusAsync(long id, bool isActive, long updatedBy);

        // ===== Guest Category Management =====
        Task<MasterDataListResponse<GuestCategoryMasterItem>> GetGuestCategoriesAsync(MasterDataListRequest request);
        Task<long> CreateGuestCategoryAsync(CreateMasterDataRequest request, long createdBy);
        Task<bool> UpdateGuestCategoryAsync(UpdateMasterDataRequest request, long updatedBy);
        Task<bool> UpdateGuestCategoryStatusAsync(long id, bool isActive, long updatedBy);

        // ===== Theme Management =====
        Task<MasterDataListResponse<ThemeMasterItem>> GetThemesAsync(MasterDataListRequest request);
        Task<long> CreateThemeAsync(CreateMasterDataRequest request, long createdBy);
        Task<bool> UpdateThemeAsync(UpdateMasterDataRequest request, long updatedBy);
        Task<bool> UpdateThemeStatusAsync(long id, bool isActive, long updatedBy);

        // ===== Common Operations =====
        Task<UsageCheckResponse> CheckUsageAsync(string tableName, string idColumn, long id);
        Task<bool> NameExistsAsync(string tableName, string nameColumn, string name, long? excludeId = null);
    }
}
