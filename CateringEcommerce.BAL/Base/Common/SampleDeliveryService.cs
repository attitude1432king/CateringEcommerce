using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Delivery;
using System;
using System.Threading.Tasks;

namespace CateringEcommerce.BAL.Base.Common
{
    /// <summary>
    /// Service for Sample Delivery (Third-party tracking)
    /// </summary>
    public class SampleDeliveryService : ISampleDeliveryService
    {
        private readonly IDatabaseHelper _dbHelper;
        private readonly SampleDeliveryRepository _repository;

        public SampleDeliveryService(IDatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper;
            _repository = new SampleDeliveryRepository(_dbHelper);
        }

        // ===================================
        // CREATE SAMPLE DELIVERY
        // ===================================
        public async Task<SampleDeliveryDto> CreateSampleDeliveryAsync(CreateSampleDeliveryRequest request)
        {
            try
            {
                if (request.OrderId <= 0)
                    throw new ArgumentException("Invalid order ID", nameof(request.OrderId));

                if (request.UserId <= 0)
                    throw new ArgumentException("Invalid user ID", nameof(request.UserId));

                if (request.OwnerId <= 0)
                    throw new ArgumentException("Invalid owner ID", nameof(request.OwnerId));

                if (string.IsNullOrWhiteSpace(request.Provider))
                    throw new ArgumentException("Provider is required", nameof(request.Provider));

                // Create delivery record
                long sampleDeliveryId = await _repository.CreateSampleDeliveryAsync(request);

                // TODO: Call third-party provider API (Dunzo/Porter/Shadowfax)
                // This is where you would integrate with the actual delivery provider
                // For now, we'll simulate the tracking info
                string trackingUrl = GenerateTrackingUrl(request.Provider, sampleDeliveryId);
                string trackingId = $"{request.Provider.ToUpper()}-{sampleDeliveryId}-{DateTime.Now:yyyyMMddHHmmss}";

                // Update with tracking info
                await _repository.UpdateTrackingInfoAsync(sampleDeliveryId, trackingUrl, trackingId);

                // Fetch and return the created delivery
                var delivery = await _repository.GetSampleDeliveryByIdAsync(sampleDeliveryId);
                if (delivery == null)
                    throw new InvalidOperationException("Failed to create sample delivery");

                return delivery;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating sample delivery: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET SAMPLE DELIVERY BY ORDER ID
        // ===================================
        public async Task<SampleDeliveryDto?> GetSampleDeliveryByOrderIdAsync(long orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Invalid order ID", nameof(orderId));

                return await _repository.GetSampleDeliveryByOrderIdAsync(orderId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving sample delivery: " + ex.Message, ex);
            }
        }

        // ===================================
        // UPDATE SAMPLE DELIVERY STATUS
        // ===================================
        public async Task<bool> UpdateSampleDeliveryStatusAsync(UpdateSampleDeliveryStatusRequest request)
        {
            try
            {
                if (request.SampleDeliveryId <= 0)
                    throw new ArgumentException("Invalid sample delivery ID", nameof(request.SampleDeliveryId));

                // Update status
                bool statusUpdated = await _repository.UpdateDeliveryStatusAsync(
                    request.SampleDeliveryId,
                    request.NewStatus
                );

                // Update tracking info if provided
                if (!string.IsNullOrWhiteSpace(request.TrackingUrl) || !string.IsNullOrWhiteSpace(request.TrackingId))
                {
                    await _repository.UpdateTrackingInfoAsync(
                        request.SampleDeliveryId,
                        request.TrackingUrl ?? string.Empty,
                        request.TrackingId ?? string.Empty
                    );
                }

                // TODO: Send notification to user about status change
                // This should integrate with your notification service

                return statusUpdated;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating sample delivery status: " + ex.Message, ex);
            }
        }

        // ===================================
        // GET TRACKING INFO
        // ===================================
        public async Task<SampleDeliveryDto?> GetTrackingInfoAsync(long orderId)
        {
            try
            {
                if (orderId <= 0)
                    throw new ArgumentException("Invalid order ID", nameof(orderId));

                var delivery = await _repository.GetSampleDeliveryByOrderIdAsync(orderId);

                // TODO: Optionally fetch real-time status from third-party provider
                // and update local status if different

                return delivery;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving tracking info: " + ex.Message, ex);
            }
        }

        // ===================================
        // HELPER: GENERATE TRACKING URL
        // ===================================
        private string GenerateTrackingUrl(string provider, long deliveryId)
        {
            // TODO: Replace with actual provider tracking URLs
            return provider.ToLower() switch
            {
                "dunzo" => $"https://tracking.dunzo.com/track/{deliveryId}",
                "porter" => $"https://porter.in/track/{deliveryId}",
                "shadowfax" => $"https://shadowfax.in/track/{deliveryId}",
                _ => $"https://tracking.provider.com/{deliveryId}"
            };
        }
    }
}
