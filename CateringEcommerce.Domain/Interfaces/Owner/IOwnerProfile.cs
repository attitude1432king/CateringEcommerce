using CateringEcommerce.Domain.Models.APIModels.Owner;
using CateringEcommerce.Domain.Models.Owner;

namespace CateringEcommerce.Domain.Interfaces.Owner
{
    public interface IOwnerProfile
    {
        Task<OwnerModel> GetOwnerDetails(long ownerPKID);
    }
}
