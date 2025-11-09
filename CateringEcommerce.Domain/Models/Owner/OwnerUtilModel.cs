namespace CateringEcommerce.Domain.Models.Owner
{
    public class OwnerUtilModel
    {
    }

    // Catering Master Type Model
    public class CateringMasterTypeModel
    {
        public int TypeId { get; set; }
        public string? TypeName { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int CategoryId { get; set; }
    }

    // Catering Master Category Model 
    public class CateringMasterCategoryModel
    {
        public int CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public string? CategoryCode { get; set; }
    }
}