using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Enums.Admin;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Admin;
using CateringEcommerce.Domain.Models.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CateringEcommerce.API.Controllers.Admin
{
    /// <summary>
    /// Admin Partner Request Approval & Rejection Controller
    /// Handles the complete workflow for reviewing and approving/rejecting partner registration requests
    /// </summary>
    [Route("api/admin/partners")]
    [ApiController]
    [AdminAuthorize]
    public class PartnerApprovalController : ControllerBase
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly string _connStr;
        private readonly ILogger<PartnerApprovalController> _logger;
        private readonly INotificationHelper _notificationHelper;
        private readonly IConfiguration _config;

        public PartnerApprovalController(IDatabaseHelper dbHelper, IConfiguration config, INotificationHelper notificationHelper, ILogger<PartnerApprovalController> logger)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _connStr = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _notificationHelper = notificationHelper ?? throw new ArgumentNullException(nameof(notificationHelper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #region Partner Request Listing

        /// <summary>
        /// GET /api/admin/partners/pending
        /// Gets list of pending partner requests with filtering and pagination
        /// </summary>
        /// <remarks>
        /// Query Parameters:
        /// - pageNumber: Page number (default: 1)
        /// - pageSize: Items per page (default: 20)
        /// - searchTerm: Search by business name, owner name, phone, or email
        /// - approvalStatusId: Filter by approval status (1=Pending, 2=Approved, 3=Rejected, 4=Under Review, 5=Info Requested)
        /// - priorityId: Filter by priority (0=Low, 1=Normal, 2=High, 3=Urgent)
        /// - cityId: Filter by city ID
        /// - fromDate: Filter by registration date from
        /// - toDate: Filter by registration date to
        /// - sortBy: Column to sort by (default: c_createddate)
        /// - sortOrder: ASC or DESC (default: DESC)
        /// </remarks>
        [HttpGet("pending")]
        public IActionResult GetPendingPartnerRequests([FromQuery] PartnerRequestFilterRequest filter)
        {
            try
            {
                var repository = new AdminPartnerApprovalRepository(_dbHelper);
                var result = repository.GetPendingPartnerRequests(filter);
                return ApiResponseHelper.Success(result, "Partner requests retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partner requests");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Partner Request Detail

        /// <summary>
        /// GET /api/admin/partners/{partnerId}/registration-detail
        /// Gets complete registration details for a specific partner request
        /// </summary>
        /// <remarks>
        /// Returns ALL data submitted during partner registration including:
        /// - Business details
        /// - Owner details
        /// - Address information
        /// - Legal compliance (FSSAI, GST, PAN)
        /// - Bank account details
        /// - Service operations
        /// - Uploaded documents and photos
        /// </remarks>
        [HttpGet("{partnerId}/registration-detail")]
        public IActionResult GetPartnerRegistrationDetail(long partnerId)
        {
            try
            {
                var repository = new AdminPartnerApprovalRepository(_dbHelper);
                var detail = repository.GetPartnerRequestDetail(partnerId);

                if (detail == null)
                    return NotFound(ApiResponseHelper.Failure("Partner request not found"));

                return ApiResponseHelper.Success(detail, "Partner request details retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving partner request detail. PartnerId: {PartnerId}", partnerId);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Approval Actions

        /// <summary>
        /// POST /api/admin/partners/{partnerId}/approve
        /// Approves a partner registration request
        /// </summary>
        /// <remarks>
        /// Validation Rules:
        /// - Partner must be in PENDING or UNDER_REVIEW status
        /// - Cannot approve already approved partners
        /// - Cannot approve rejected partners without re-registration
        ///
        /// Actions Performed:
        /// - Updates c_approval_status to 2 (APPROVED)
        /// - Sets c_approved_date to current timestamp
        /// - Sets c_approved_by to current admin ID
        /// - Sets c_verified_by_admin to true
        /// - Sets c_isactive to true
        /// - Partner becomes visible in the system
        /// </remarks>
        [HttpPost("{partnerId}/approve")]
        public IActionResult ApprovePartnerRequest(long partnerId, [FromBody] ApprovePartnerRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token"));

                var repository = new AdminPartnerApprovalRepository(_dbHelper);
                var result = repository.ApprovePartnerRequest(partnerId, adminId.Value, request.Remarks);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Partner request approved. PartnerId: {PartnerId}, AdminId: {AdminId}",
                        partnerId, adminId.Value);

                    // Send approval notification to partner (email/SMS)
                    if (request.SendNotification)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await SendPartnerApprovalNotificationAsync(partnerId);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send approval notification for PartnerId: {PartnerId}", partnerId);
                            }
                        });
                    }

                    return Ok(ApiResponseHelper.Success(result, result.Message));
                }

                _logger.LogWarning(
                    "Failed to approve partner request. PartnerId: {PartnerId}, Reason: {Reason}",
                    partnerId, result.Message);

                return BadRequest(ApiResponseHelper.Failure(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error approving partner request. PartnerId: {PartnerId}", partnerId);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Rejection Actions

        /// <summary>
        /// POST /api/admin/partners/{partnerId}/reject
        /// Rejects a partner registration request
        /// </summary>
        /// <remarks>
        /// Validation Rules:
        /// - Rejection reason is MANDATORY
        /// - Cannot reject already approved partners
        /// - Can reject partners in PENDING, UNDER_REVIEW, or INFO_REQUESTED status
        ///
        /// Actions Performed:
        /// - Updates c_approval_status to 3 (REJECTED)
        /// - Sets c_approved_date to current timestamp (for audit)
        /// - Sets c_approved_by to current admin ID
        /// - Stores c_rejection_reason
        /// - Partner remains inactive in the system
        /// </remarks>
        [HttpPost("{partnerId}/reject")]
        public IActionResult RejectPartnerRequest(long partnerId, [FromBody] RejectPartnerRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.RejectionReason))
                    return BadRequest(ApiResponseHelper.Failure("Rejection reason is required"));

                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token"));

                var repository = new AdminPartnerApprovalRepository(_dbHelper);
                var result = repository.RejectPartnerRequest(partnerId, adminId.Value, request.RejectionReason);

                if (result.Success)
                {
                    _logger.LogInformation(
                        "Partner request rejected. PartnerId: {PartnerId}, AdminId: {AdminId}, Reason: {Reason}",
                        partnerId, adminId.Value, request.RejectionReason);

                    // Send rejection notification to partner (email/SMS)
                    if (request.SendNotification)
                    {
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await SendPartnerRejectionNotificationAsync(partnerId, request.RejectionReason);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Failed to send rejection notification for PartnerId: {PartnerId}", partnerId);
                            }
                        });
                    }

                    return Ok(ApiResponseHelper.Success(result, result.Message));
                }

                _logger.LogWarning(
                    "Failed to reject partner request. PartnerId: {PartnerId}, Reason: {Reason}",
                    partnerId, result.Message);

                return BadRequest(ApiResponseHelper.Failure(result.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting partner request. PartnerId: {PartnerId}", partnerId);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Priority Management

        /// <summary>
        /// PUT /api/admin/partners/{partnerId}/priority
        /// Updates the priority of a partner request
        /// </summary>
        /// <remarks>
        /// Priority Values:
        /// - 0: Low
        /// - 1: Normal (default)
        /// - 2: High
        /// - 3: Urgent
        /// </remarks>
        [HttpPut("{partnerId}/priority")]
        public IActionResult UpdatePriority(long partnerId, [FromBody] UpdatePriorityRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token"));

                // Validate priority enum value
                if (!EnumHelper.IsValidEnumValue<PriorityStatus>(request.PriorityId))
                    return BadRequest(ApiResponseHelper.Failure("Invalid priority value"));

                var priority = EnumHelper.GetEnumFromInt<PriorityStatus>(request.PriorityId);
                if (!priority.HasValue)
                    return BadRequest(ApiResponseHelper.Failure("Invalid priority value"));

                var repository = new AdminPartnerApprovalRepository(_dbHelper);
                var result = repository.UpdatePriority(partnerId, priority.Value, adminId.Value);

                if (result)
                {
                    _logger.LogInformation(
                        "Priority updated. PartnerId: {PartnerId}, NewPriority: {Priority}, AdminId: {AdminId}",
                        partnerId, EnumHelper.GetDisplayName(priority.Value), adminId.Value);

                    return Ok(ApiResponseHelper.Success(null, "Priority updated successfully"));
                }

                return BadRequest(ApiResponseHelper.Failure("Failed to update priority"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating priority. PartnerId: {PartnerId}", partnerId);
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Enum Reference Endpoints (for UI dropdowns)

        /// <summary>
        /// GET /api/admin/partners/enums/approval-statuses
        /// Gets all approval status options for UI dropdowns
        /// </summary>
        [HttpGet("enums/approval-statuses")]
        public IActionResult GetApprovalStatuses()
        {
            try
            {
                var statuses = EnumHelper.GetEnumDictionary<ApprovalStatus>()
                    .Select(kvp => new ApprovalStatusOption
                    {
                        Id = kvp.Key,
                        Name = kvp.Value
                    })
                    .ToList();

                return Ok(ApiResponseHelper.Success(statuses, "Approval statuses retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving approval statuses");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// GET /api/admin/partners/enums/priorities
        /// Gets all priority options for UI dropdowns
        /// </summary>
        [HttpGet("enums/priorities")]
        public IActionResult GetPriorities()
        {
            try
            {
                var priorities = EnumHelper.GetEnumDictionary<PriorityStatus>()
                    .Select(kvp => new PriorityStatusOption
                    {
                        Id = kvp.Key,
                        Name = kvp.Value
                    })
                    .ToList();

                return Ok(ApiResponseHelper.Success(priorities, "Priorities retrieved successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving priorities");
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        #endregion

        #region Helper Methods

        private long? GetAdminIdFromToken()
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AdminId" || c.Type == "userId")?.Value;
            if (long.TryParse(adminIdClaim, out var adminId))
                return adminId;

            return null;
        }

        /// <summary>
        /// Sends multi-channel notification (Email + SMS) when partner is approved
        /// </summary>
        private async Task SendPartnerApprovalNotificationAsync(long partnerId)
        {
            try
            {
                // Get partner details
                var repository = new AdminPartnerApprovalRepository(_dbHelper);
                var partnerDetail = repository.GetPartnerRequestDetail(partnerId);

                if (partnerDetail == null)
                {
                    _logger.LogWarning("Cannot send approval notification: Partner {PartnerId} not found", partnerId);
                    return;
                }

                // Prepare notification data
                var notificationData = new Dictionary<string, object>
                {
                    { "owner_id", partnerId },
                    { "owner_name", partnerDetail.OwnerName },
                    { "catering_name", partnerDetail.BusinessName },
                    { "approval_date", partnerDetail.ApprovedDate?.ToString("dd MMM yyyy") ?? DateTime.Now.ToString("dd MMM yyyy") },
                    { "login_url", _config["AppSettings:PartnerPortalUrl"] ?? "https://partner.enyvora.com/login" },
                    { "username", partnerDetail.Email },
                    { "temp_password", "Please use the password you set during registration" },
                    { "partner_guide_url", _config["AppSettings:PartnerGuideUrl"] ?? "https://enyvora.com/partner-guide" },
                    { "best_practices_url", _config["AppSettings:BestPracticesUrl"] ?? "https://enyvora.com/best-practices" },
                    { "support_url", _config["AppSettings:SupportUrl"] ?? "https://enyvora.com/support" },
                    { "partner_support_email", _config["AppSettings:PartnerSupportEmail"] ?? "support@enyvora.com" },
                    { "partner_support_phone", _config["AppSettings:PartnerSupportPhone"] ?? "+91-1234567890" }
                };

                // Send multi-channel notification (Email + SMS)
                await _notificationHelper.SendMultiChannelNotificationAsync(
                    templateCodePrefix: "PARTNER_APPROVAL",
                    audience: "PARTNER",
                    recipientId: partnerId.ToString(),
                    recipientEmail: partnerDetail.Email,
                    recipientPhone: partnerDetail.Phone,
                    data: notificationData,
                    sendEmail: true,
                    sendSms: true,
                    sendInApp: false,
                    priority: NotificationPriority.High
                );

                _logger.LogInformation(
                    "Partner approval notification sent successfully. PartnerId: {PartnerId}, Email: {Email}, Phone: {Phone}",
                    partnerId, partnerDetail.Email, partnerDetail.Phone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending partner approval notification for PartnerId: {PartnerId}", partnerId);
                throw;
            }
        }

        /// <summary>
        /// Sends multi-channel notification (Email + SMS) when partner is rejected
        /// </summary>
        private async Task SendPartnerRejectionNotificationAsync(long partnerId, string rejectionReason)
        {
            try
            {
                // Get partner details
                var repository = new AdminPartnerApprovalRepository(_dbHelper);
                var partnerDetail = repository.GetPartnerRequestDetail(partnerId);

                if (partnerDetail == null)
                {
                    _logger.LogWarning("Cannot send rejection notification: Partner {PartnerId} not found", partnerId);
                    return;
                }

                // Prepare notification data
                var notificationData = new Dictionary<string, object>
                {
                    { "owner_id", partnerId },
                    { "owner_name", partnerDetail.OwnerName },
                    { "catering_name", partnerDetail.BusinessName },
                    { "rejection_reason", rejectionReason },
                    { "reapply_duration", _config["AppSettings:ReapplyDuration"] ?? "30 days" },
                    { "partner_support_email", _config["AppSettings:PartnerSupportEmail"] ?? "support@enyvora.com" },
                    { "partner_support_phone", _config["AppSettings:PartnerSupportPhone"] ?? "+91-1234567890" }
                };


                // Send multi-channel notification (Email + SMS)
                await _notificationHelper.SendMultiChannelNotificationAsync(
                    templateCodePrefix: "PARTNER_REJECTION",
                    audience: "PARTNER",
                    recipientId: partnerId.ToString(),
                    recipientEmail: partnerDetail.Email,
                    recipientPhone: partnerDetail.Phone,
                    data: notificationData,
                    sendEmail: true,
                    sendSms: true,
                    sendInApp: false,
                    priority: NotificationPriority.Normal
                );

                _logger.LogInformation(
                    "Partner rejection notification sent successfully. PartnerId: {PartnerId}, Email: {Email}, Phone: {Phone}",
                    partnerId, partnerDetail.Email, partnerDetail.Phone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending partner rejection notification for PartnerId: {PartnerId}", partnerId);
                throw;
            }
        }

        #endregion
    }
}
