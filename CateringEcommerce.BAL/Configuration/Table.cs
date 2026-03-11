using Microsoft.Identity.Client;

namespace CateringEcommerce.BAL.Configuration
{
    public static class Table
    {
        #region Common Tables
        public const string State = "t_sys_state";
        public const string City = "t_sys_city";

        // Master Data Tables
        public const string SysGuestCategory = "t_sys_guest_category";
        public const string SysCateringServiceCategory = "t_sys_catering_type_category";
        public const string SysCateringTypeMaster = "t_sys_catering_type_master";
        public const string SysFoodCategory = "t_sys_food_category";
        public const string SysCateringDocumentTypes = "t_sys_catering_document_types"; 
        #endregion

        #region User Tables
        public const string SysUser = "t_sys_user";
        public const string SysUserAddresses = "t_sys_user_addresses";
        public const string SysUserCart = "t_sys_user_cart";
        public const string SysCartFoodItems = "t_sys_cart_food_items";
        public const string SysUserFavorites = "t_sys_user_favorites";
        #endregion

        #region Owner Tables
        public const string SysCateringOwner = "t_sys_catering_owner";
        public const string SysCateringOwnerAddress = "t_sys_catering_owner_addresses";
        public const string SysCateringOwnerService = "t_sys_catering_owner_operations";
        public const string SysCateringOwnerLegal = "t_sys_catering_owner_compliance";
        public const string SysCateringOwnerImages = "t_sys_catering_owner_images";
        public const string SysCateringOwnerBankDetails = "t_sys_catering_owner_bankdetails";
        public const string SysCateringOwnerAgreement = "t_sys_catering_owner_agreement";
        public const string SysCateringMediaUploads = "t_sys_catering_media_uploads";
        public const string SysCateringReview = "t_sys_catering_review";
        public const string SysCateringReviewReply = "t_sys_owner_review_reply";

        // Menu Management
        public const string SysMenuPackage = "t_sys_catering_packages";
        public const string SysMenuPackageItems = "t_sys_catering_package_items";
        public const string SysFoodItems = "t_sys_fooditems";

        // Decorations
        public const string SysCateringDecorations = "t_sys_catering_decorations";
        public const string SysCateringThemeTypes = "t_sys_catering_theme_types"; // Alias for consistency

        // Staff Management
        public const string SysCateringStaff = "t_sys_catering_staff";

        // Discounts
        public const string SysCateringDiscount = "t_sys_catering_discount";
        public const string SysCateringDiscountItemMapping = "t_map_discount_fooditem";
        public const string SysCateringDiscountPackageMapping = "t_map_discount_package";

        // Availability Management
        public const string SysCateringAvailabilityGlobal = "t_catering_availability_global";
        public const string SysCateringAvailabilityDate = "t_catering_availability_dates";

        // Homepage
        public const string SysHomepageStats = "t_sys_homepage_stats";

        // Banner Management
        public const string SysCateringBanners = "t_sys_catering_banners";

        // Order Management
        public const string SysOrders = "t_sys_orders";
        public const string SysOrderItems = "t_sys_order_items";
        public const string SysOrderStatusHistory = "t_sys_order_status_history";
        public const string SysOrderPayments = "t_sys_order_payments";
        public const string SysOrderPaymentSummary = "t_sys_order_payment_summary";
        public const string SysOrderPaymentStages = "t_sys_order_payment_stages";
        public const string SysOrderModifications = "t_sys_order_modifications";
        public const string SysAutoLockJobs = "t_sys_auto_lock_jobs";
        public const string SysEventLocations = "t_sys_event_locations";

        // Invoice Management (New - Added 2026-02-20)
        public const string SysInvoice = "t_sys_invoice";
        public const string SysInvoiceLineItems = "t_sys_invoice_line_items";
        public const string SysPaymentSchedule = "t_sys_payment_schedule";
        public const string SysInvoiceAuditLog = "t_sys_invoice_audit_log";

        // Delivery Management
        public const string SysSampleDelivery = "t_sys_sample_delivery";
        public const string SysEventDelivery = "t_sys_event_delivery";
        public const string SysEventDeliveryHistory = "t_sys_event_delivery_history";

        // Order Actions Tables  
        public const string SysCancellationRequests = "t_sys_cancellation_requests";
        public const string SysOrderComplaints = "t_sys_order_complaints";

        // Payment Tables 
        public const string SysPaymentSummary = "t_sys_payment_summary";
        public const string SysPaymentTransantions = "t_sys_payment_transactions";
        public const string SysPartnerSecurityDeposits = "t_sys_partner_security_deposits";
        public const string SysDepositTransactions = "t_sys_deposit_transactions";
        public const string SysEscrowLedger = "t_sys_escrow_ledger";
        public const string SysEMIPlan = "t_sys_emi_plans";
        public const string SysPartnerPayoutRequests = "t_sys_partner_payout_requests";
        public const string SysPaymentGatewayConfig = "t_sys_payment_gateway_config";

        // Partnership & Commission Tables
        public const string SysPartnershipTiers = "t_sys_partnership_tiers";
        public const string SysOwnerPartnershipTiers = "t_sys_partnership_tiers"; // Backward-compatible alias
        public const string SysCommissionTierHistory = "t_sys_commission_tier_history";
        public const string SysOwnerSecurityDeposits = "t_sys_partner_security_deposits"; // Backward-compatible alias

        // OAuth & Security Tables
        public const string SysOAuthProvider = "t_sys_oauth_provider";
        public const string SysOAuthState = "t_sys_oauth_state";
        public const string SysUserOAuth = "t_sys_user_oauth";
        public const string SysUser2FA = "t_sys_user_2fa";
        public const string SysOwner2FA = "t_sys_owner_2fa";
        public const string SysTrustedDevice = "t_sys_trusted_device";
        public const string Sys2FAAttemptLog = "t_sys_2fa_attempt_log";

        // Discounts
        public const string SysCateringDiscountUsage = "t_sys_catering_discount_usage";

        // Notification Tables
        public const string SysNotificationDelivery = "t_sys_notification_delivery";
        public const string SysNotifications = "t_sys_notifications";
        #endregion

        #region Admin Tables
        public const string SysAdmin = "t_sys_admin";
        public const string SysAdminActivityLog = "t_sys_admin_activity_log";
        public const string SysAdminNotifications = "t_sys_admin_notifications";

        // Partner Actions Tables 
        public const string SysPartnerRequestActions = "t_sys_partner_request_actions";
        public const string SysPartnerRequestCommunications = "t_sys_partner_request_communications";
        

        // RBAC Tables
        public const string SysAdminUsers = "t_sys_admin_users";
        public const string SysAdminRoles = "t_sys_admin_roles";
        public const string SysAdminPermissions = "t_sys_admin_permissions";
        public const string SysAdminRolePermissions = "t_sys_admin_role_permissions";
        public const string SysAdminUserRoles = "t_sys_admin_user_roles"; // Deprecated, kept for compatibility
        public const string SysAdminAuditLogs = "t_sys_admin_audit_logs";

        // Settings & Configuration Tables
        public const string SysSettings = "t_sys_settings";
        public const string SysSettingsHistory = "t_sys_settings_history";
        public const string SysCommissionConfig = "t_sys_commission_config";
        public const string SysTemplateVariables = "t_sys_template_variables";
        public const string SysNotificationTemplates = "t_sys_notification_templates";
        #endregion

        #region Support Tables
        public const string SysSupportTickets = "t_sys_support_tickets";
        public const string SysSupportTicketMessages = "t_sys_support_ticket_messages";
        #endregion

        #region Supervisor Tables
        public const string SysSupervisor = "t_sys_supervisor";
        public const string SysSupervisorRegistration = "t_sys_supervisor_registration";
        public const string SysSupervisorAssignment = "t_sys_supervisor_assignment";
        public const string SysSupervisorActionLog = "t_sys_supervisor_action_log";
        public const string SysPreEventChecklist = "t_sys_pre_event_checklist";
        public const string SysDuringEventTracking = "t_sys_during_event_tracking";
        public const string SysPostEventReport = "t_sys_post_event_report";
        #endregion
    }
}
