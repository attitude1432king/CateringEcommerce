namespace CateringEcommerce.Domain.Models.User;

public class UserModel
{
    public long PkID { get; set; }
    public string? FullName { get; set; }
    public required string Phone { get; set; }
    public string? Email { get; set; }
    public bool IsEmailVerified { get; set; }
    public bool IsPhoneVerified { get; set; }
    public int CityID { get; set; }
    public int StateID { get; set; }
    public string? Description { get; set; }
    public string? ProfilePhoto { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
}