using CateringEcommerce.Domain.Models.APIModels.Owner;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    /// <summary>
    /// Owner profile management operations
    /// </summary>
    public interface IOwnerProfile
    {
        Task<OwnerModel> GetOwnerDetails(long ownerPKID);
        string GetLogoPath(long ownerPKID);
        Task UpdateOwnerBusiness(long ownerPKID, BusinessSettingsDto businessDto);
        Task UpdateCateringAddress(long ownerPKID, AddressSettingsDto addressDto);
        Task UpdateCateringServices(long ownerPKID, ServicesSettingsDto servicesDto);
        Task UpdateLegalAndBankDetails(long ownerPKID, LegalPaymentSettingsDto legalDto);
    }
}
