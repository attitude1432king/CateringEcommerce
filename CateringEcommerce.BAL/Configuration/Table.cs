namespace CateringEcommerce.BAL.Configuration
{
    public static class Table
    {
        #region Common Tables
        public const string State = "t_sys_state";
        public const string City = "t_sys_city";
        #endregion

        #region User Tables
        public const string SysUser = "t_sys_user";
        #endregion

        #region Owner Tables
        public const string SysCateringOwner = "t_sys_catering_owner";
        public const string SysCateringOwnerAddress = "t_sys_catering_owner_addresses";
        public const string SysCateringOwnerService = "t_sys_catering_owner_operations";
        public const string SysCateringOwnerLegal = "t_sys_catering_owner_compliance";
        public const string SysCateringOwnerImages = "t_sys_catering_owner_images";
        public const string SysCateringOwnerBankDetails = "t_sys_catering_owner_bankdetails";
        public const string SysCateringServiceCategory = "t_sys_catering_type_category";
        public const string SysCateringTypeMaster = "t_sys_catering_type_master";
        public const string SysCateringMediaUploads = "t_sys_catering_media_uploads";
        public const string SysCateringReview = "t_sys_catering_review";

        // Menu Management
        public const string SysFoodCategory = "t_sys_food_category";
        public const string SysMenuPackage = "t_sys_catering_packages";
        public const string SysMenuPackageItems = "t_sys_catering_package_items";
        public const string SysFoodItems = "t_sys_fooditems";

        // Decorations 
        public const string SysCateringDecorations = "t_sys_catering_decorations";
        public const string SysDecorationThemes = "t_sys_catering_theme_types";

        // Staff Management
        public const string SysCateringStaff = "t_sys_catering_staff";

        // Discounts
        public const string SysCateringDiscount = "t_sys_catering_discount";
        public const string SysCateringDiscountItemMapping = "t_map_discount_fooditem";
        public const string SysCateringDiscountPackageMapping = "t_map_discount_package";

        // Availability Management
        public const string SysCateringAvailabilityGlobal = "t_catering_availability_global";
        public const string SysCateringAvailabilityDate = "t_catering_availability_dates";
        #endregion
    }
}
