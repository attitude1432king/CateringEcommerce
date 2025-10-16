namespace CateringEcommerce.Domain.Models;

public class UserModel
{
    public Int64 PkID { get; set; }
    public string FullName { get; set; }
    public required string Phone { get; set; }
    public string Email { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public int CityID { get; set; }
    public int StateID { get; set; }
    public string Description { get; set; }
    public string ProfilePhoto { get; set; }
    public string Role { get; set; }
}