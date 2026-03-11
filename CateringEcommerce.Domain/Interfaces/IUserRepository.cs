using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface IUserRepository
    {
        UserModel GetUserDetails(Int64 userPKID);
        bool IsExistEmail(string email, string role = "User");
        bool IsExistNumber(string phoneNumber, string role);
        bool IsExistRoleBaseNumber(string phoneNumber, string type, string role);
        
        /// <summary>
        /// Get approval status for partner/owner by phone number
        /// </summary>
        int? GetOwnerApprovalStatus(string phoneNumber);
        
        /// <summary>
        /// Check if owner exists and get their details along with approval status
        /// </summary>
        (bool exists, int? approvalStatus) CheckOwnerWithApprovalStatus(string phoneNumber);
    }
}
