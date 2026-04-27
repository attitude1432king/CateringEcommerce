using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IOwnerRegister
    {
        Task<Int64> CreateOwnerAccount(Dictionary<string, object> dicData);
        Task RegisterAddress(Int64 onwerId, Dictionary<string, object> dicData);
        Task RegisterServices(Int64 ownerId, Dictionary<string, object> dicData);
        Task RegisterLegalDocuments(Int64 ownerId, Dictionary<string, object> dicData);
        Task RegisterBankDetails(Int64 ownerId, Dictionary<string, object> dicData);
        Task UpdateLogoPath(Int64 ownerPkid, string logoPath);
        Task RegisterAgreement(Int64 ownerId, Dictionary<string, object> dicData, string baseUploadPath);
        Task<List<ServiceTypeDetails>> GetServiceDetailsByTypeId(int serviceTypeId);
    }
}
