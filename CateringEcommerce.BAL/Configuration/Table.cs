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

        #endregion
    }
}
