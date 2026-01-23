using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IOwnerRepository
    {
       // public Task<Models.OwnerModel> GetOwnerDetailsAsync(Int64 ownerPkID);

        Task<bool> IsOwnerExistAsync(Int64 ownerPkID);
        public bool IsEmailExist(string email);
        public bool IsOwnerPhoneExist(string phone);

        Task<int> SaveFilePath(string filePath, Int64 ownerPkid, string fileName, DocumentType documentType = DocumentType.Menu, long referenceID = 0);
        Task<int> DeleteDocumentFile(long documentPKID);
        Task<int> SoftDeleteDocumentFile(long documentPKID);
        Task<int> UpdateDocumentFilePath(long referenceID, DocumentType documentType, string filePath);
        Task<int> SoftDeleteByReferenceID(long referenceID, DocumentType documentType);
    }
}
