using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IOwnerRegister
    {
        Int64 CreateOwnerAccount(Dictionary<string, object> dicData);
        void RegisterAddress(Int64 onwerId, Dictionary<string, object> dicData);
        void RegisterServices(Int64 ownerId, Dictionary<string, object> dicData);
        void RegisterLegalDocuments(Int64 ownerId, Dictionary<string, object> dicData);
        void RegisterBankDetails(Int64 ownerId, Dictionary<string, object> dicData);
        void UpdateLogoPath(Int64 ownerPkid, string logoPath);
        void RegisterAgreement(Int64 ownerId, Dictionary<string, object> dicData, string baseUploadPath);
        Task<List<ServiceTypeDetails>> GetServiceDetailsByTypeId(int serviceTypeId);
    }
}
