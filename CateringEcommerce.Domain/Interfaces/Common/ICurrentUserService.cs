namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface ICurrentUserService
    {
        long UserId { get; }
        string PhoneNumber { get; }
        string UserRole { get; }
    }
}
