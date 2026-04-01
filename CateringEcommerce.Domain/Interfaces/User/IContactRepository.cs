using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces.User
{
    public interface IContactRepository
    {
        bool SaveMessage(ContactMessageRequest request, string? ipAddress);
    }
}
