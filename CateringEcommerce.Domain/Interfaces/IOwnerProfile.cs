using CateringEcommerce.Domain.Models;
using CateringEcommerce.Domain.Models.APIModels.Owner;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IOwnerProfile
    {
        Task UpdateOwnerProfile(long ownerPKID, UpdateOwnerProfileDto updateOwnerProfileDto);
        OwnerModel GetOwnerDetails(long ownerPKID);
    }
}
