namespace CateringEcommerce.Domain.Interfaces
{
    public interface ICurrentUserService
    {
        Int64 UserId { get; }
        string PhoneNumber { get; }
        string UserRole { get; }
    }
}
