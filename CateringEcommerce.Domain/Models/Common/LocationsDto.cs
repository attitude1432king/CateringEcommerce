namespace CateringEcommerce.Domain.Models.Common
{
    public class LocationsDto
    {
        public string City { get; init; } = "";
        public string State { get; init; } = "";
        public string Country { get; init; } = "";
        public string Source { get; init; } = ""; // IP | COOKIE | DEFAULT
    }

    public sealed class GeoCityResult
    {
        public string City { get; init; } = "";
        public string State { get; init; } = "";
        public string Country { get; init; } = "";
    }
}
