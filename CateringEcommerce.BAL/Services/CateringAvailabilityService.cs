using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Enums;
using CateringEcommerce.Domain.Models.User;

namespace CateringEcommerce.BAL.Services
{
    public class CateringAvailabilityService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly ISystemSettingsProvider _settingsProvider;

        public CateringAvailabilityService(
            IOrderRepository orderRepository,
            ISystemSettingsProvider settingsProvider)
        {
            _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
        }

        public int GetMinimumAdvanceBookingDays()
        {
            return _settingsProvider.GetInt("BUSINESS.MIN_ADVANCE_BOOKING_DAYS", 5);
        }

        public async Task<CateringAvailabilityResponseDto?> GetAvailabilityAsync(long cateringId, DateTime selectedDate)
        {
            var snapshot = await _orderRepository.GetCateringAvailabilitySnapshotAsync(cateringId, selectedDate);
            if (snapshot == null || !snapshot.Exists)
            {
                return null;
            }

            if (snapshot.DailyBookingCapacity <= 0)
            {
                snapshot.DailyBookingCapacity = _settingsProvider.GetInt("BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY", 1);
            }

            var availableSlots = Math.Max(0, snapshot.DailyBookingCapacity - snapshot.ExistingBookingCount);
            var response = new CateringAvailabilityResponseDto
            {
                IsAvailable = true,
                Message = "Available for booking",
                AvailableSlots = availableSlots
            };

            if (!snapshot.IsApproved || !snapshot.IsActive)
            {
                response.IsAvailable = false;
                response.Message = "Catering service is not available";
                response.AvailableSlots = 0;
                return response;
            }

            if (snapshot.GlobalStatus == AvailabilityStatus.CLOSED)
            {
                response.IsAvailable = false;
                response.Message = "Catering service is closed on the selected date";
                response.AvailableSlots = 0;
                return response;
            }

            if (snapshot.DateStatus == AvailabilityStatus.CLOSED)
            {
                response.IsAvailable = false;
                response.Message = "Not available";
                response.AvailableSlots = 0;
                return response;
            }

            if (snapshot.DateStatus == AvailabilityStatus.FULLY_BOOKED)
            {
                response.IsAvailable = false;
                response.Message = "Not available";
                response.AvailableSlots = 0;
                return response;
            }

            if (availableSlots <= 0)
            {
                response.IsAvailable = false;
                response.Message = "Not available";
                response.AvailableSlots = 0;
                return response;
            }

            if (availableSlots <= 2)
            {
                response.Message = "Limited availability";
            }
            else if (selectedDate.DayOfWeek == DayOfWeek.Saturday || selectedDate.DayOfWeek == DayOfWeek.Sunday)
            {
                response.Message = "High demand day";
            }

            return response;
        }

        public async Task<List<DateTime>?> GetBlockedDatesAsync(long cateringId, int year, int month)
        {
            var snapshot = await _orderRepository.GetCateringAvailabilitySnapshotAsync(cateringId, new DateTime(year, month, 1));
            if (snapshot == null || !snapshot.Exists)
            {
                return null;
            }

            if (snapshot.GlobalStatus == AvailabilityStatus.CLOSED)
            {
                int daysInMonth = DateTime.DaysInMonth(year, month);
                return Enumerable.Range(1, daysInMonth)
                    .Select(day => new DateTime(year, month, day))
                    .ToList();
            }

            int fallbackCapacity = snapshot.DailyBookingCapacity > 0
                ? snapshot.DailyBookingCapacity
                : _settingsProvider.GetInt("BUSINESS.DEFAULT_DAILY_BOOKING_CAPACITY", 1);

            return await _orderRepository.GetUnavailableCateringDatesAsync(cateringId, year, month, fallbackCapacity);
        }
    }
}
