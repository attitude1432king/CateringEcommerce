using CateringEcommerce.API.Helpers;
using CateringEcommerce.BAL.Base.Owner.Dashboard;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Owner;
using CateringEcommerce.API.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Owner.Dashboard
{
    /// <summary>
    /// Owner Support Controller
    /// Provides support ticket management for partner owners
    /// </summary>
    [OwnerAuthorize]
    [ApiController]
    [Route("api/Owner/[controller]")]
    public class OwnerSupportController : ControllerBase
    {
        private readonly ILogger<OwnerSupportController> _logger;
        private readonly ICurrentUserService _currentUser;
        private readonly OwnerSupportRepository _supportRepository;

        public OwnerSupportController(
            ILogger<OwnerSupportController> logger,
            ICurrentUserService currentUser,
            OwnerSupportRepository supportRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _supportRepository = supportRepository ?? throw new ArgumentNullException(nameof(supportRepository));
        }

        /// <summary>
        /// Create a new support ticket
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateTicket([FromBody] CreateSupportTicketDto dto)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                    return ApiResponseHelper.Failure("Owner not authenticated.");

                if (string.IsNullOrWhiteSpace(dto.Subject) || string.IsNullOrWhiteSpace(dto.Description))
                    return ApiResponseHelper.Failure("Subject and description are required.");

                if (string.IsNullOrWhiteSpace(dto.Category))
                    return ApiResponseHelper.Failure("Category is required.");

                _logger.LogInformation($"Owner {ownerId} creating support ticket: {dto.Subject}");

                var ticket = await _supportRepository.CreateTicket(ownerId, dto);

                return ApiResponseHelper.Success(ticket, "Support ticket created successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating support ticket");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while creating the support ticket."));
            }
        }

        /// <summary>
        /// Get filtered and paginated tickets list
        /// </summary>
        [HttpPost("list")]
        public async Task<IActionResult> GetTickets([FromBody] SupportTicketFilterDto filter)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                    return ApiResponseHelper.Failure("Owner not authenticated.");

                _logger.LogInformation($"Getting support tickets for owner {ownerId}, page: {filter.Page}");

                var tickets = await _supportRepository.GetTickets(ownerId, filter);

                return ApiResponseHelper.Success(tickets, "Support tickets retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting support tickets");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving support tickets."));
            }
        }

        /// <summary>
        /// Get ticket details with messages
        /// </summary>
        [HttpGet("{ticketId}")]
        public async Task<IActionResult> GetTicketDetail(long ticketId)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                    return ApiResponseHelper.Failure("Owner not authenticated.");

                _logger.LogInformation($"Getting ticket detail for owner {ownerId}, ticket: {ticketId}");

                var detail = await _supportRepository.GetTicketDetail(ownerId, ticketId);

                if (detail == null)
                    return ApiResponseHelper.Failure("Ticket not found or does not belong to this owner.");

                return ApiResponseHelper.Success(detail, "Ticket details retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ticket detail for ticket {ticketId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving ticket details."));
            }
        }

        /// <summary>
        /// Send a message on a ticket
        /// </summary>
        [HttpPost("{ticketId}/message")]
        public async Task<IActionResult> SendMessage(long ticketId, [FromBody] SendTicketMessageDto dto)
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                    return ApiResponseHelper.Failure("Owner not authenticated.");

                if (string.IsNullOrWhiteSpace(dto.MessageText))
                    return ApiResponseHelper.Failure("Message text cannot be empty.");

                _logger.LogInformation($"Owner {ownerId} sending message on ticket {ticketId}");

                var message = await _supportRepository.SendMessage(ownerId, ticketId, dto.MessageText);

                if (message == null)
                    return ApiResponseHelper.Failure("Ticket not found or does not belong to this owner.");

                return ApiResponseHelper.Success(message, "Message sent successfully.");
            }
            catch (InvalidOperationException ex)
            {
                return ApiResponseHelper.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending message on ticket {ticketId}");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while sending the message."));
            }
        }

        /// <summary>
        /// Get ticket statistics
        /// </summary>
        [HttpGet("stats")]
        public async Task<IActionResult> GetTicketStats()
        {
            try
            {
                long ownerId = _currentUser.UserId;
                if (ownerId <= 0)
                    return ApiResponseHelper.Failure("Owner not authenticated.");

                var stats = await _supportRepository.GetTicketStats(ownerId);

                return ApiResponseHelper.Success(stats, "Ticket statistics retrieved successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ticket stats");
                return StatusCode(500, ApiResponseHelper.Failure("An error occurred while retrieving ticket statistics."));
            }
        }
    }
}
