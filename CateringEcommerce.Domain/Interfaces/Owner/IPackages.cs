using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IPackages
    {
        Task<long> AddPackage(long ownerPKID, PackageDto packageDto);
        Task UpdatePackage(long? packagePKID, PackageDto packageDto);
        Task DeletePackage(long? packagePKID);
        Task AddPackageItems(long? packagePKID, PackageItemDto packageItem);
        Task UpdatePackageItems(long? packagePKID, PackageItemDto packageItem);
        Task DeletePackageItems(long? packagePKID, long packageItemId = 0);
        void UpdatePackageStatus(long? packagePKID, bool isActive);
        Task<List<PackageDto>> GetPackages(long ownerPKID);
        Task<List<PackageItemDto>> GetPackageItems(long packagePKID);
        Task<List<FoodCategoryDto>> GetCategories();
        //bool PackageItemExistOrNot(long packageItemId);
        bool PackageExistOrNot(long ownerPKID, string packageName);

    }
}
