using CateringEcommerce.Domain.Interfaces.Supervisor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Hubs
{
    /// <summary>
    /// Real-Time Supervisor Event Tracking Hub
    /// Handles live updates for supervisors, partners, and admins during events
    /// </summary>
    [Authorize]
    public class SupervisorTrackingHub : Hub
    {
        private readonly ILogger<SupervisorTrackingHub> _logger;
        private readonly ISupervisorAssignmentRepository _assignmentRepo;

        public SupervisorTrackingHub(
            ILogger<SupervisorTrackingHub> logger,
            ISupervisorAssignmentRepository assignmentRepo)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _assignmentRepo = assignmentRepo ?? throw new ArgumentNullException(nameof(assignmentRepo));
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = Context.User?.FindFirst("UserType")?.Value; // SUPERVISOR, PARTNER, ADMIN

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
            {
                _logger.LogWarning("SupervisorTrackingHub: Connection rejected - Missing user identity");
                Context.Abort();
                return;
            }

            // Add to user-specific group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"{userType}_{userId}");

            // Add to role-specific group
            await Groups.AddToGroupAsync(Context.ConnectionId, userType);

            _logger.LogInformation(
                "SupervisorTrackingHub: User {UserId} ({UserType}) connected. ConnectionId: {ConnectionId}",
                userId, userType, Context.ConnectionId);

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = Context.User?.FindFirst("UserType")?.Value;

            _logger.LogInformation(
                "SupervisorTrackingHub: User {UserId} ({UserType}) disconnected. ConnectionId: {ConnectionId}",
                userId, userType, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to real-time updates for a specific assignment
        /// </summary>
        public async Task SubscribeToAssignment(long assignmentId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return;

            await Groups.AddToGroupAsync(Context.ConnectionId, $"Assignment_{assignmentId}");

            _logger.LogInformation(
                "User {UserId} subscribed to assignment {AssignmentId}",
                userId, assignmentId);
        }

        /// <summary>
        /// Unsubscribe from assignment updates
        /// </summary>
        public async Task UnsubscribeFromAssignment(long assignmentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Assignment_{assignmentId}");
        }

        /// <summary>
        /// Broadcast supervisor check-in to all relevant parties
        /// Called by EventSupervisionController after check-in
        /// </summary>
        public async Task BroadcastCheckIn(long assignmentId, long supervisorId, string location, DateTime checkInTime)
        {
            await Clients.Group($"Assignment_{assignmentId}").SendAsync("SupervisorCheckedIn", new
            {
                assignmentId,
                supervisorId,
                location,
                checkInTime,
                message = "Supervisor has checked in to the event"
            });

            _logger.LogInformation(
                "Broadcasted check-in for assignment {AssignmentId}, supervisor {SupervisorId}",
                assignmentId, supervisorId);
        }

        /// <summary>
        /// Broadcast event progress update (guest count, food serving, etc.)
        /// </summary>
        public async Task BroadcastEventProgress(long assignmentId, string updateType, object data)
        {
            await Clients.Group($"Assignment_{assignmentId}").SendAsync("EventProgressUpdate", new
            {
                assignmentId,
                updateType, // "GUEST_COUNT", "FOOD_SERVING", "EXTRA_QUANTITY"
                data,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Broadcasted event progress for assignment {AssignmentId}: {UpdateType}",
                assignmentId, updateType);
        }

        /// <summary>
        /// Broadcast issue notification to admin and partner
        /// </summary>
        public async Task BroadcastIssue(long assignmentId, string issueType, string description, string severity)
        {
            // Notify assignment-specific subscribers
            await Clients.Group($"Assignment_{assignmentId}").SendAsync("IssueReported", new
            {
                assignmentId,
                issueType,
                description,
                severity, // "LOW", "MEDIUM", "HIGH", "CRITICAL"
                timestamp = DateTime.UtcNow
            });

            // Notify all admins immediately for high/critical issues
            if (severity == "HIGH" || severity == "CRITICAL")
            {
                await Clients.Group("ADMIN").SendAsync("CriticalIssueAlert", new
                {
                    assignmentId,
                    issueType,
                    description,
                    severity
                });
            }

            _logger.LogWarning(
                "Issue reported for assignment {AssignmentId}: {IssueType} ({Severity})",
                assignmentId, issueType, severity);
        }

        /// <summary>
        /// Broadcast post-event completion
        /// </summary>
        public async Task BroadcastEventCompletion(long assignmentId, int overallRating, bool issuesFound)
        {
            await Clients.Group($"Assignment_{assignmentId}").SendAsync("EventCompleted", new
            {
                assignmentId,
                overallRating,
                issuesFound,
                completedAt = DateTime.UtcNow,
                message = "Event supervision completed. Awaiting admin verification."
            });

            _logger.LogInformation(
                "Broadcasted event completion for assignment {AssignmentId}, rating: {Rating}",
                assignmentId, overallRating);
        }

        /// <summary>
        /// Broadcast payment status update
        /// </summary>
        public async Task BroadcastPaymentUpdate(long assignmentId, long supervisorId, string status, decimal amount)
        {
            // Notify supervisor
            await Clients.Group($"SUPERVISOR_{supervisorId}").SendAsync("PaymentStatusUpdate", new
            {
                assignmentId,
                status, // "REQUESTED", "APPROVED", "REJECTED", "RELEASED"
                amount,
                timestamp = DateTime.UtcNow
            });

            _logger.LogInformation(
                "Broadcasted payment update for assignment {AssignmentId}: {Status}",
                assignmentId, status);
        }
    }
}
