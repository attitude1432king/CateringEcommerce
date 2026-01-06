using CateringEcommerce.Domain.Enums;

namespace CateringEcommerce.Domain.Models.Owner
{
    public class GlobalAvailabilityModel
    {
        public string GlobalStatus { get; set; } = "OPEN"; // OPEN | CLOSED

        // Key = yyyy-MM-dd
        public Dictionary<string, DateAvailabilityPayload> SpecialDates { get; set; }
            = new();
    }

    public class DateAvailabilityPayload
    {
        public string Status { get; set; } = ""; // OPEN | CLOSED | FULLY_BOOKED
        public string? Note { get; set; }
    }

    public class DateAvailabilityDTO
    {
        public DateTime Date { get; set; }
        public string? Status { get; set; } = "OPEN"; // OPEN | CLOSED | FULLY_BOOKED
        public string? Note { get; set; }
    }

}
