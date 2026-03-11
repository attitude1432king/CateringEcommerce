using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Admin;
using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Helpers;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Admin;
using CateringEcommerce.Domain.Interfaces.Notification;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/partner-requests")]
    [ApiController]
    [AdminAuthorize]
    public class AdminPartnerRequestsController : ControllerBase
    {
        private readonly IAdminPartnerRequestRepository _partnerRequestRepository;
        private readonly IAdminNotificationRepository _notificationRepository;
        private readonly INotificationHelper _notificationHelper;
        private readonly ILogger<AdminPartnerRequestsController> _logger;

        public AdminPartnerRequestsController(
            IAdminPartnerRequestRepository partnerRequestRepository,
            IAdminNotificationRepository notificationRepository,
            INotificationHelper notificationHelper,
            ILogger<AdminPartnerRequestsController> logger)
        {
            _notificationHelper = notificationHelper ?? throw new ArgumentNullException(nameof(notificationHelper));
            _partnerRequestRepository = partnerRequestRepository ?? throw new ArgumentNullException(nameof(partnerRequestRepository));
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all partner requests with filtering and pagination
        /// </summary>
        [HttpGet]
        public IActionResult GetAllPartnerRequests([FromQuery] AdminPartnerRequestListRequest request)
        {
            try
            {
                var result = _partnerRequestRepository.GetAllPartnerRequests(request);
                return ApiResponseHelper.Success(result, "Partner requests retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Get partner request details by Owner ID
        /// </summary>
        [HttpGet("{ownerId}")]
        public IActionResult GetPartnerRequestById(long ownerId)
        {
            try
            {
                var partnerRequest = _partnerRequestRepository.GetPartnerRequestById(ownerId);

                if (partnerRequest == null)
                    return ApiResponseHelper.Failure("Partner request not found.");

                // Log the view action
                var adminId = GetAdminIdFromToken();
                if (adminId.HasValue)
                {
                    _partnerRequestRepository.LogAction(ownerId, adminId.Value, "VIEWED", null, null, "Viewed partner request details", GetClientIpAddress());
                }

                return ApiResponseHelper.Success(partnerRequest, "Partner request details retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Update partner request status (Unified endpoint for all actions)
        /// </summary>
        [HttpPut("{ownerId}/status")]
        public IActionResult UpdateStatus(long ownerId, [FromBody] PartnerRequestActionRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                request.OwnerId = ownerId;

                PartnerRequestActionResponse result;

                switch (request.ActionType?.ToUpper())
                {
                    case "APPROVE":
                        result = _partnerRequestRepository.ApprovePartnerRequest(request, adminId.Value);
                        break;
                    case "REJECT":
                        result = _partnerRequestRepository.RejectPartnerRequest(request, adminId.Value);
                        break;
                    case "REQUEST_INFO":
                        result = _partnerRequestRepository.RequestAdditionalInfo(request, adminId.Value);
                        break;
                    case "MARK_UNDER_REVIEW":
                        _partnerRequestRepository.UpdatePartnerRequestStatus(ownerId, "UNDER_REVIEW", adminId.Value, request.Remarks);
                        result = new PartnerRequestActionResponse
                        {
                            Success = true,
                            Message = "Status updated to Under Review",
                            NewStatus = "UNDER_REVIEW"
                        };
                        break;
                    default:
                        return BadRequest(ApiResponseHelper.Failure("Invalid action type. Use APPROVE, REJECT, REQUEST_INFO, or MARK_UNDER_REVIEW."));
                }

                if (result.Success)
                {
                    // Create notification for certain actions
                    if (request.ActionType?.ToUpper() == "APPROVE")
                    {
                        _notificationRepository.CreateNotification(
                            "PARTNER_REQUEST_APPROVED",
                            $"Partner request approved for {ownerId}",
                            "A partner registration has been approved",
                            ownerId,
                            "PARTNER_REQUEST",
                            $"/admin/partner-requests?id={ownerId}",
                            null
                        );
                    }

                    return ApiResponseHelper.Success(result, result.Message);
                }

                return ApiResponseHelper.Failure(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Approve a partner request
        /// </summary>
        [HttpPut("{ownerId}/approve")]
        public async Task<IActionResult> ApprovePartnerRequest(long ownerId, [FromBody] PartnerRequestActionRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                request.OwnerId = ownerId;
                request.ActionType = "APPROVE";

                var result = _partnerRequestRepository.ApprovePartnerRequest(request, adminId.Value);

                if (result.Success)
                {
                    // Get partner details for notification
                    var partnerDetails = _partnerRequestRepository.GetPartnerRequestById(ownerId);
                    if (partnerDetails != null)
                    {
                        try
                        {
                            // Generate cryptographically secure temporary password
                            var temporaryPassword = Utils.GenerateSecureTemporaryPassword(16);

                            // TODO: Store hashed password in database for partner login
                            // _partnerRequestRepository.SetPartnerTemporaryPassword(ownerId, BCrypt.Net.BCrypt.HashPassword(temporaryPassword));
                            // _partnerRequestRepository.MarkPasswordAsTemporary(ownerId); // Force password change on first login

                            await _notificationHelper.SendPartnerNotificationAsync(
                                "PARTNER_APPROVAL",
                                partnerDetails.OwnerName,
                                partnerDetails.Email,
                                partnerDetails.Phone,
                                new Dictionary<string, object>
                                {
                                    { "owner_name", partnerDetails.OwnerName },
                                    { "catering_name", partnerDetails.BusinessName },
                                    { "approval_date", DateTime.Now.ToString("dd MMM yyyy") },
                                    { "login_url", "https://enyvora.com/partner/login" },
                                    { "username", partnerDetails.Email },
                                    { "temp_password", temporaryPassword }, // Secure generated password
                                    { "password_expiry_warning", "This password will expire in 24 hours. Please change it upon first login." },
                                    { "partner_guide_url", "https://enyvora.com/partner-guide" },
                                    { "best_practices_url", "https://enyvora.com/best-practices" },
                                    { "support_url", "https://enyvora.com/support" },
                                    { "partner_support_email", "partner-support@enyvora.com" },
                                    { "partner_support_phone", "+91-1234567890" }
                                }
                            );
                            _logger.LogInformation("Partner approval notification sent to {OwnerName}. OwnerId: {OwnerId}",
                                partnerDetails.OwnerName, ownerId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send partner approval notification. OwnerId: {OwnerId}", ownerId);
                        }
                    }

                    // Create admin notification
                    _notificationRepository.CreateNotification(
                        "PARTNER_REQUEST_APPROVED",
                        $"Partner request approved for {ownerId}",
                        "A partner registration has been approved",
                        ownerId,
                        "PARTNER_REQUEST",
                        $"/admin/partner-requests?id={ownerId}",
                        null // Send to all admins
                    );

                    return ApiResponseHelper.Success(result, result.Message);
                }

                return ApiResponseHelper.Failure(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Reject a partner request
        /// </summary>
        [HttpPut("{ownerId}/reject")]
        public async Task<IActionResult> RejectPartnerRequest(long ownerId, [FromBody] PartnerRequestActionRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                request.OwnerId = ownerId;
                request.ActionType = "REJECT";

                var result = _partnerRequestRepository.RejectPartnerRequest(request, adminId.Value);

                if (result.Success)
                {
                    // Get partner details for notification
                    var partnerDetails = _partnerRequestRepository.GetPartnerRequestById(ownerId);
                    if (partnerDetails != null)
                    {
                        try
                        {
                            // Send rejection notification to partner (Email + SMS)

                            await _notificationHelper.SendPartnerNotificationAsync(
                                "PARTNER_REJECTION",
                                partnerDetails.OwnerName,
                                partnerDetails.Email,
                                partnerDetails.Phone,
                                new Dictionary<string, object>
                                {
                                    { "owner_name", partnerDetails.OwnerName },
                                    { "catering_name", partnerDetails.BusinessName },
                                    { "rejection_reason", request.Remarks ?? "Application did not meet our current requirements" },
                                    { "reapply_duration", "30 days" },
                                    { "partner_support_email", "partner-support@enyvora.com" }
                                }
                            );
                            _logger.LogInformation("Partner rejection notification sent to {OwnerName}. OwnerId: {OwnerId}",
                                partnerDetails.OwnerName, ownerId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send partner rejection notification. OwnerId: {OwnerId}", ownerId);
                        }
                    }

                    return ApiResponseHelper.Success(result, result.Message);
                }

                return ApiResponseHelper.Failure(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Request additional information from partner
        /// </summary>
        [HttpPut("{ownerId}/request-info")]
        public async Task<IActionResult> RequestAdditionalInfo(long ownerId, [FromBody] PartnerRequestActionRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                request.OwnerId = ownerId;
                request.ActionType = "REQUEST_INFO";

                var result = _partnerRequestRepository.RequestAdditionalInfo(request, adminId.Value);

                if (result.Success)
                {
                    // Get partner details for notification
                    var partnerDetails = _partnerRequestRepository.GetPartnerRequestById(ownerId);
                    if (partnerDetails != null)
                    {
                        try
                        {
                            // Send info request notification to partner (Email + SMS)
                            await _notificationHelper.SendPartnerNotificationAsync(
                                "PARTNER_INFO_REQUEST",
                                partnerDetails.OwnerName,
                                partnerDetails.Email,
                                partnerDetails.Phone,
                                new Dictionary<string, object>
                                {
                                    { "owner_name", partnerDetails.OwnerName },
                                    { "catering_name", partnerDetails.BusinessName },
                                    { "info_requested", request.Remarks ?? "Additional documents and information required" },
                                    { "deadline_date", DateTime.Now.AddDays(7).ToString("dd MMM yyyy") },
                                    { "upload_url", "https://enyvora.com/partner/upload-documents" },
                                    { "partner_support_email", "partner-support@enyvora.com" }
                                }
                            );
                            _logger.LogInformation("Partner info request notification sent to {OwnerName}. OwnerId: {OwnerId}",
                                partnerDetails.OwnerName, ownerId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to send partner info request notification. OwnerId: {OwnerId}", ownerId);
                        }
                    }

                    return ApiResponseHelper.Success(result, result.Message);
                }

                return ApiResponseHelper.Failure(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Update internal notes for a partner request
        /// </summary>
        [HttpPut("{ownerId}/notes")]
        public IActionResult UpdateInternalNotes(long ownerId, [FromBody] UpdateNotesRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                var result = _partnerRequestRepository.UpdateInternalNotes(ownerId, request.Notes, adminId.Value);

                if (result)
                    return ApiResponseHelper.Success(null, "Internal notes updated successfully.");

                return ApiResponseHelper.Failure("Failed to update internal notes.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Update priority for a partner request
        /// </summary>
        [HttpPut("{ownerId}/priority")]
        public IActionResult UpdatePriority(long ownerId, [FromBody] UpdatePriorityRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                var result = _partnerRequestRepository.UpdatePriority(ownerId, request.PriorityId, adminId.Value);

                if (result)
                    return ApiResponseHelper.Success(null, "Priority updated successfully.");

                return ApiResponseHelper.Failure("Failed to update priority.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Get action log/timeline for a partner request
        /// </summary>
        [HttpGet("{ownerId}/timeline")]
        public IActionResult GetActionLog(long ownerId)
        {
            try
            {
                var timeline = _partnerRequestRepository.GetActionLog(ownerId);

                return ApiResponseHelper.Success(timeline, "Action log retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Send communication to partner
        /// </summary>
        [HttpPost("{ownerId}/communicate")]
        public IActionResult SendCommunication(long ownerId, [FromBody] PartnerCommunicationRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                request.OwnerId = ownerId;

                var result = _partnerRequestRepository.SendCommunication(request, adminId.Value);

                if (result.Success)
                    return ApiResponseHelper.Success(result, result.Message);

                return ApiResponseHelper.Failure(result.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        /// <summary>
        /// Get communication history for a partner request
        /// </summary>
        [HttpGet("{ownerId}/communications")]
        public IActionResult GetCommunicationHistory(long ownerId)
        {
            try
            {
                var history = _partnerRequestRepository.GetCommunicationHistory(ownerId);

                return ApiResponseHelper.Success(history, "Communication history retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Admin Partner Request operation failed");
                return StatusCode(500, ApiResponseHelper.Failure("An internal error occurred. Please try again later."));
            }
        }

        #region Helper Methods

        private long? GetAdminIdFromToken()
        {
            var adminIdClaim = User.Claims.FirstOrDefault(c => c.Type == "AdminId" || c.Type == "userId")?.Value;
            if (long.TryParse(adminIdClaim, out var adminId))
                return adminId;

            return null;
        }

        private string? GetClientIpAddress()
        {
            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        #endregion
    }

    #region Request Models

    public class UpdateNotesRequest
    {
        public string Notes { get; set; } = string.Empty;
    }

    public class UpdatePriorityRequest
    {
        public int PriorityId { get; set; }
    }

    #endregion
}
