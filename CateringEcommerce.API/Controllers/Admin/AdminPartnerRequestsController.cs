using CateringEcommerce.API.Filters;
using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Common.Admin;
using CateringEcommerce.Domain.Models.Admin;
using Microsoft.AspNetCore.Mvc;

namespace CateringEcommerce.API.Controllers.Admin
{
    [Route("api/admin/partner-requests")]
    [ApiController]
    [AdminAuthorize]
    public class AdminPartnerRequestsController : ControllerBase
    {
        private readonly string _connStr;

        public AdminPartnerRequestsController(IConfiguration config)
        {
            _connStr = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        /// <summary>
        /// Get all partner requests with filtering and pagination
        /// </summary>
        [HttpGet]
        public IActionResult GetAllPartnerRequests([FromQuery] AdminPartnerRequestListRequest request)
        {
            try
            {
                var repository = new AdminPartnerRequestRepository(_connStr);
                var result = repository.GetAllPartnerRequests(request);
                return ApiResponseHelper.Success(result, "Partner requests retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
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
                var repository = new AdminPartnerRequestRepository(_connStr);
                var partnerRequest = repository.GetPartnerRequestById(ownerId);

                if (partnerRequest == null)
                    return ApiResponseHelper.Failure("Partner request not found.");

                // Log the view action
                var adminId = GetAdminIdFromToken();
                if (adminId.HasValue)
                {
                    repository.LogAction(ownerId, adminId.Value, "VIEWED", null, null, "Viewed partner request details", GetClientIpAddress());
                }

                return ApiResponseHelper.Success(partnerRequest, "Partner request details retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
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

                var repository = new AdminPartnerRequestRepository(_connStr);
                PartnerRequestActionResponse result;

                switch (request.ActionType?.ToUpper())
                {
                    case "APPROVE":
                        result = repository.ApprovePartnerRequest(request, adminId.Value);
                        break;
                    case "REJECT":
                        result = repository.RejectPartnerRequest(request, adminId.Value);
                        break;
                    case "REQUEST_INFO":
                        result = repository.RequestAdditionalInfo(request, adminId.Value);
                        break;
                    case "MARK_UNDER_REVIEW":
                        repository.UpdatePartnerRequestStatus(ownerId, "UNDER_REVIEW", adminId.Value, request.Remarks);
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
                        var notificationRepo = new AdminNotificationRepository(_connStr);
                        notificationRepo.CreateNotification(
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
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Approve a partner request
        /// </summary>
        [HttpPut("{ownerId}/approve")]
        public IActionResult ApprovePartnerRequest(long ownerId, [FromBody] PartnerRequestActionRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                request.OwnerId = ownerId;
                request.ActionType = "APPROVE";

                var repository = new AdminPartnerRequestRepository(_connStr);
                var result = repository.ApprovePartnerRequest(request, adminId.Value);

                if (result.Success)
                {
                    // Create notification for partner approval
                    var notificationRepo = new AdminNotificationRepository(_connStr);
                    notificationRepo.CreateNotification(
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
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Reject a partner request
        /// </summary>
        [HttpPut("{ownerId}/reject")]
        public IActionResult RejectPartnerRequest(long ownerId, [FromBody] PartnerRequestActionRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                request.OwnerId = ownerId;
                request.ActionType = "REJECT";

                var repository = new AdminPartnerRequestRepository(_connStr);
                var result = repository.RejectPartnerRequest(request, adminId.Value);

                if (result.Success)
                    return ApiResponseHelper.Success(result, result.Message);

                return ApiResponseHelper.Failure(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
            }
        }

        /// <summary>
        /// Request additional information from partner
        /// </summary>
        [HttpPut("{ownerId}/request-info")]
        public IActionResult RequestAdditionalInfo(long ownerId, [FromBody] PartnerRequestActionRequest request)
        {
            try
            {
                var adminId = GetAdminIdFromToken();
                if (!adminId.HasValue)
                    return Unauthorized(ApiResponseHelper.Failure("Admin ID not found in token."));

                request.OwnerId = ownerId;
                request.ActionType = "REQUEST_INFO";

                var repository = new AdminPartnerRequestRepository(_connStr);
                var result = repository.RequestAdditionalInfo(request, adminId.Value);

                if (result.Success)
                    return ApiResponseHelper.Success(result, result.Message);

                return ApiResponseHelper.Failure(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
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

                var repository = new AdminPartnerRequestRepository(_connStr);
                var result = repository.UpdateInternalNotes(ownerId, request.Notes, adminId.Value);

                if (result)
                    return ApiResponseHelper.Success(null, "Internal notes updated successfully.");

                return ApiResponseHelper.Failure("Failed to update internal notes.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
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

                var repository = new AdminPartnerRequestRepository(_connStr);
                var result = repository.UpdatePriority(ownerId, request.Priority, adminId.Value);

                if (result)
                    return ApiResponseHelper.Success(null, "Priority updated successfully.");

                return ApiResponseHelper.Failure("Failed to update priority.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
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
                var repository = new AdminPartnerRequestRepository(_connStr);
                var timeline = repository.GetActionLog(ownerId);

                return ApiResponseHelper.Success(timeline, "Action log retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
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

                var repository = new AdminPartnerRequestRepository(_connStr);
                var result = repository.SendCommunication(request, adminId.Value);

                if (result.Success)
                    return ApiResponseHelper.Success(result, result.Message);

                return ApiResponseHelper.Failure(result.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
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
                var repository = new AdminPartnerRequestRepository(_connStr);
                var history = repository.GetCommunicationHistory(ownerId);

                return ApiResponseHelper.Success(history, "Communication history retrieved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponseHelper.Failure($"Internal server error: {ex.Message}"));
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
        public string Priority { get; set; } = "NORMAL";
    }

    #endregion
}
