using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IMediaRepository
    {
        Task<List<MediaFileModel>> GetMediaFiles(long ownerPKID, DocumentType documentTypeID, long referenceID = 0);
    }
}
