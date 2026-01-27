using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Common;

namespace CateringEcommerce.BAL.Common
{
    public sealed class IpApiGeoLocationService : IGeoLocationService
    {
        private readonly HttpClient _http;

        public IpApiGeoLocationService(HttpClient http)
        {
            _http = http;
        }

        public async Task<GeoCityResult?> ResolveCityAsync(string ipAddress)
        {
            if (ipAddress == "127.0.0.1")
                return new GeoCityResult
                {
                    City = "Surat",
                    State = "Gujarat",
                    Country = "India"
                };

            var response = await _http.GetFromJsonAsync<IpApiResponse>(
                $"https://ipapi.co/{ipAddress}/json/");

            if (response == null || string.IsNullOrWhiteSpace(response.city))
                return null;

            return new GeoCityResult
            {
                City = Sanitize(response.city),
                State = Sanitize(response.region),
                Country = Sanitize(response.country_name)
            };
        }

        private static string Sanitize(string? value)
            => value?.Trim().Replace("<", "").Replace(">", "") ?? "";
    }

    internal sealed class IpApiResponse
    {
        public string city { get; set; } = "";
        public string region { get; set; } = "";
        public string country_name { get; set; } = "";
    }

}
