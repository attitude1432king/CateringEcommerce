using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Models.User
{
    public class CateringAvailabilityResponseDto
    {
        public bool IsAvailable { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AvailableSlots { get; set; }
    }

    public class CateringAvailabilitySnapshotDto
    {
        public long CateringId { get; set; }
        public bool Exists { get; set; }
        public bool IsApproved { get; set; }
        public bool IsActive { get; set; }
        public AvailabilityStatus GlobalStatus { get; set; } = AvailabilityStatus.OPEN;
        public AvailabilityStatus? DateStatus { get; set; }
        public int DailyBookingCapacity { get; set; }
        public int ExistingBookingCount { get; set; }
    }
}
