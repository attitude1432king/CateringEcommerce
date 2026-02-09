using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Delivery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Common
{
    /// <summary>
    /// Service for Event Catering Delivery (Status-based, NO GPS)
    /// </summary>
    public class EventDeliveryService : IEventDeliveryService
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly EventDeliveryRepository _repository;

        public EventDeliveryService(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _repository = new EventDeliveryRepository(_dbHelper);
        }

        // ===================================
        // INITIALIZE EVENT DELIVERY
        // ===================================
        public async Task<EventDeliveryDto> InitEventDeliveryAsync(InitEventDeliveryRequest request)
        {
            try
            {
                if (request.OrderId <= 0)
                    throw new ArgumentException("Invalid order ID", nameof(request.OrderId));

                if (request.OwnerId <= 0)
                    throw new ArgumentException("Invalid owner ID", nameof(request.OwnerId));

                // Check if delivery already exists for this order
                var existing = await _repository.GetEventDeliveryByOrderIdAsync(request.OrderId);
                if (existing != null)
                    throw new InvalidOperationException("Event delivery already exists for this order");

                // Create delivery record
                long eventDeliveryId = await _repository.CreateEventDeliveryAsync(request);

                // Add initial status history
                await _repository.AddStatusHistoryAsync(new EventDeliveryHistoryDto
                {
                    EventDeliveryId = eventDeliveryId,
                    OrderId = request.OrderId,
                    PreviousStatus = null,
                    NewStatus = EventDeliveryStatus.PreparationStarted,
                    ChangedByUserId = request.OwnerId,
                    ChangedByType = "Partner",
                    Notes = "Event delivery initialized"
                });

                // TODO: Send notification to user
                // NotificationService.SendAsync("Food preparation has started")

                // Fetch and return the created delivery
                var delivery = await _repository.GetEventDeliveryByIdAsync(eventDeliveryId);
                if (delivery == null)
                    throw new InvalidOperationException("Failed to create event delivery");

                return delivery;
            }
            catch (Exception ex)
            {
                throw new Exception("Error initializing event delivery: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET EVENT DELIVERY BY ORDER ID
        // ===================================
        public async Task<EventDeliveryDto?> GetEventDeliveryByOrderIdAsync(long orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Invalid order ID", nameof(orderId));

                return await _repository.GetEventDeliveryByOrderIdAsync(orderId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving event delivery: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE EVENT DELIVERY STATUS
        // ===================================
        public async Task<EventDeliveryDto> UpdateEventDeliveryStatusAsync(UpdateEventDeliveryStatusRequest request)
        {
            try
            {
                if (request.EventDeliveryId <= 0)
                    throw new ArgumentException("Invalid event delivery ID", nameof(request.EventDeliveryId));

                // Get current delivery
                var delivery = await _repository.GetEventDeliveryByIdAsync(request.EventDeliveryId);
                if (delivery == null)
                    throw new InvalidOperationException("Event delivery not found");

                // Validate status transition
                if (!IsValidStatusTransition(delivery.DeliveryStatus, request.NewStatus))
                {
                    throw new InvalidOperationException(
                        $"Invalid status transition from {delivery.DeliveryStatus} to {request.NewStatus}. " +
                        "Status must follow the sequence: Preparation Started → Vehicle Ready → Dispatched → Arrived At Venue → Event Completed"
                    );
                }

                // Update delivery status
                bool updated = await _repository.UpdateDeliveryStatusAsync(request.EventDeliveryId, request);

                if (!updated)
                    throw new InvalidOperationException("Failed to update delivery status");

                // Add status history
                await _repository.AddStatusHistoryAsync(new EventDeliveryHistoryDto
                {
                    EventDeliveryId = request.EventDeliveryId,
                    OrderId = delivery.OrderId,
                    PreviousStatus = delivery.DeliveryStatus,
                    NewStatus = request.NewStatus,
                    ChangedByUserId = request.ChangedByUserId,
                    ChangedByType = request.ChangedByType ?? "Partner",
                    Notes = request.Notes
                });

                // TODO: Send notification based on new status
                await SendStatusChangeNotificationAsync(delivery.OrderId, request.NewStatus);

                // Fetch and return updated delivery
                var updatedDelivery = await _repository.GetEventDeliveryByIdAsync(request.EventDeliveryId);
                if (updatedDelivery == null)
                    throw new InvalidOperationException("Failed to fetch updated delivery");

                return updatedDelivery;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating event delivery status: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET DELIVERY TIMELINE
        // ===================================
        public async Task<DeliveryTimelineResponse> GetDeliveryTimelineAsync(long orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Invalid order ID", nameof(orderId));

                var delivery = await _repository.GetEventDeliveryByOrderIdAsync(orderId);
                if (delivery == null)
                {
                    return new DeliveryTimelineResponse
                    {
                        EventDelivery = null,
                        StatusHistory = new List<EventDeliveryHistoryDto>(),
                        CurrentStatusText = "Not Started",
                        CanAdvanceStatus = false,
                        NextAllowedStatus = null
                    };
                }

                var history = await _repository.GetStatusHistoryAsync(delivery.EventDeliveryId);

                // Determine next allowed status
                EventDeliveryStatus? nextStatus = delivery.DeliveryStatus switch
                {
                    EventDeliveryStatus.PreparationStarted => EventDeliveryStatus.VehicleReady,
                    EventDeliveryStatus.VehicleReady => EventDeliveryStatus.Dispatched,
                    EventDeliveryStatus.Dispatched => EventDeliveryStatus.ArrivedAtVenue,
                    EventDeliveryStatus.ArrivedAtVenue => EventDeliveryStatus.EventCompleted,
                    EventDeliveryStatus.EventCompleted => null,
                    _ => null
                };

                return new DeliveryTimelineResponse
                {
                    EventDelivery = delivery,
                    StatusHistory = history,
                    CurrentStatusText = GetStatusDisplayText(delivery.DeliveryStatus),
                    CanAdvanceStatus = nextStatus.HasValue,
                    NextAllowedStatus = nextStatus
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving delivery timeline: " + ex.Message, ex);
            }
        }

        // ===================================
        // VALIDATE STATUS TRANSITION
        // ===================================
        public bool IsValidStatusTransition(EventDeliveryStatus currentStatus, EventDeliveryStatus newStatus)
        {
            // Valid transitions only
            return (currentStatus, newStatus) switch
            {
                (EventDeliveryStatus.PreparationStarted, EventDeliveryStatus.VehicleReady) => true,
                (EventDeliveryStatus.VehicleReady, EventDeliveryStatus.Dispatched) => true,
                (EventDeliveryStatus.Dispatched, EventDeliveryStatus.ArrivedAtVenue) => true,
                (EventDeliveryStatus.ArrivedAtVenue, EventDeliveryStatus.EventCompleted) => true,
                _ => false
            };
        }

        // ===================================
        // GET PARTNER ACTIVE DELIVERIES
        // ===================================
        public async Task<List<EventDeliveryDto>> GetPartnerActiveDeliveriesAsync(long ownerId)
        {
            try
            {
                if (ownerId <= 0)
                    throw new ArgumentException("Invalid owner ID", nameof(ownerId));

                return await _repository.GetActiveDeliveriesByOwnerAsync(ownerId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving partner active deliveries: " + ex.Message, ex);
            }
        }

        // ===================================
        // ADMIN: GET DELIVERY MONITOR
        // ===================================
        public async Task<List<AdminDeliveryMonitorDto>> GetAdminDeliveryMonitorAsync()
        {
            try
            {
                return await _repository.GetAllDeliveriesForMonitoringAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving admin delivery monitor: " + ex.Message, ex);
            }
        }

        // ===================================
        // ADMIN: OVERRIDE STATUS
        // ===================================
        public async Task<EventDeliveryDto> AdminOverrideStatusAsync(
            long eventDeliveryId,
            EventDeliveryStatus newStatus,
            long adminUserId,
            string notes)
        {
            try
            {
                if (eventDeliveryId <= 0)
                    throw new ArgumentException("Invalid event delivery ID", nameof(eventDeliveryId));

                if (adminUserId <= 0)
                    throw new ArgumentException("Invalid admin user ID", nameof(adminUserId));

                // Get current delivery
                var delivery = await _repository.GetEventDeliveryByIdAsync(eventDeliveryId);
                if (delivery == null)
                    throw new InvalidOperationException("Event delivery not found");

                // Admin can override any transition (no validation)
                var request = new UpdateEventDeliveryStatusRequest
                {
                    EventDeliveryId = eventDeliveryId,
                    NewStatus = newStatus,
                    ChangedByUserId = adminUserId,
                    ChangedByType = "Admin",
                    Notes = $"[ADMIN OVERRIDE] {notes}"
                };

                bool updated = await _repository.UpdateDeliveryStatusAsync(eventDeliveryId, request);

                if (!updated)
                    throw new InvalidOperationException("Failed to override delivery status");

                // Add status history with admin flag
                await _repository.AddStatusHistoryAsync(new EventDeliveryHistoryDto
                {
                    EventDeliveryId = eventDeliveryId,
                    OrderId = delivery.OrderId,
                    PreviousStatus = delivery.DeliveryStatus,
                    NewStatus = newStatus,
                    ChangedByUserId = adminUserId,
                    ChangedByType = "Admin",
                    Notes = $"[ADMIN OVERRIDE] {notes}"
                });

                // Send notification
                await SendStatusChangeNotificationAsync(delivery.OrderId, newStatus);

                // Fetch and return updated delivery
                var updatedDelivery = await _repository.GetEventDeliveryByIdAsync(eventDeliveryId);
                if (updatedDelivery == null)
                    throw new InvalidOperationException("Failed to fetch updated delivery");

                return updatedDelivery;
            }
            catch (Exception ex)
            {
                throw new Exception("Error overriding event delivery status: " + ex.Message, ex);
            }
        }

        // ===================================
        // HELPER: GET STATUS DISPLAY TEXT
        // ===================================
        private string GetStatusDisplayText(EventDeliveryStatus status)
        {
            return status switch
            {
                EventDeliveryStatus.PreparationStarted => "Food Preparation Started",
                EventDeliveryStatus.VehicleReady => "Vehicle Ready for Dispatch",
                EventDeliveryStatus.Dispatched => "Food is on the Way",
                EventDeliveryStatus.ArrivedAtVenue => "Food Arrived at Venue",
                EventDeliveryStatus.EventCompleted => "Event Completed",
                _ => "Unknown Status"
            };
        }

        // ===================================
        // HELPER: SEND NOTIFICATION
        // ===================================
        private async Task SendStatusChangeNotificationAsync(long orderId, EventDeliveryStatus newStatus)
        {
            // TODO: Integrate with notification service
            // This should send push notifications, SMS, or email to the user

            string message = newStatus switch
            {
                EventDeliveryStatus.PreparationStarted => "Food preparation has started for your event",
                EventDeliveryStatus.VehicleReady => "Food vehicle is ready for dispatch",
                EventDeliveryStatus.Dispatched => "Food is on the way to your event venue",
                EventDeliveryStatus.ArrivedAtVenue => "Food has arrived at the venue",
                EventDeliveryStatus.EventCompleted => "Your event has been completed",
                _ => "Delivery status updated"
            };

            // Placeholder for notification service call
            // await _notificationService.SendAsync(orderId, message);

            await Task.CompletedTask;
        }
    }
}
