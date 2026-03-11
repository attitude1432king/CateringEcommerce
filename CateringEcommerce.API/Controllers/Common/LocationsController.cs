using CateringEcommerce.BAL.Common;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Threading.Tasks;

namespace CateringEcommerce.API.Controllers.Common
{
    [ApiController]
    [Route("api/Common/Locations")]
    public class LocationsController : ControllerBase
    {
        private const string CityCookieKey = "user_city";

        private readonly ILocation _locationService;
        private readonly IGeoLocationService _geoService;
        private readonly IMemoryCache _cache;

        public LocationsController(
            ILocation locationService,
            IGeoLocationService geoService,
            IMemoryCache cache)
        {
            _locationService = locationService ?? throw new ArgumentNullException(nameof(locationService));
            _geoService = geoService ?? throw new ArgumentNullException(nameof(geoService));
            _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        }

        [AllowAnonymous]
        [HttpGet("states")]
        public async Task<IActionResult> GetStates()
        {
            try
            {
                var states = await _locationService.GetStates();
                return Ok(states);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching states.", ex); // Internal Server Error
            }
        }

        [AllowAnonymous]
        [HttpGet("cities/{stateId}")]
        public async Task<IActionResult> GetCities(int stateId)
        {
            try
            {
                var cities = await _locationService.GetCities(stateId);
                return Ok(cities);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while fetching cities for state '{stateId}'.", ex); // Internal Server Error
            }
        }

        [HttpGet("default-city")]
        public async Task<ActionResult<LocationsDto>> GetDefaultCity()
        {
            // 1️⃣ COOKIE CHECK
            if (Request.Cookies.TryGetValue(CityCookieKey, out var cookieCity))
            {
                return Ok(new LocationsDto
                {
                    City = cookieCity,
                    Source = "COOKIE"
                });
            }

            // 2️⃣ IP DETECTION
            var ip = ClientIpResolver.GetClientIp(HttpContext);

            if (_cache.TryGetValue(ip, out GeoCityResult cachedCity))
            {
                SetCityCookie(cachedCity.City);
                return Ok(ToDto(cachedCity, "IP"));
            }

            // 3️⃣ GEO LOOKUP
            var geoResult = await _geoService.ResolveCityAsync(ip);

            if (geoResult != null)
            {
                _cache.Set(ip, geoResult, TimeSpan.FromHours(6));
                SetCityCookie(geoResult.City);
                return Ok(ToDto(geoResult, "IP"));
            }

            // 4️⃣ FALLBACK
            var fallback = new GeoCityResult
            {
                City = "Surat",
                State = "Gujarat",
                Country = "India"
            };

            SetCityCookie(fallback.City);
            return Ok(ToDto(fallback, "DEFAULT"));
        }

        [AllowAnonymous]
        [HttpGet("pincode/{pincode}")]
        public async Task<IActionResult> GetPincodeDetails(string pincode)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(
                $"https://api.postalpincode.in/pincode/{pincode}"
            );

            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }
        private void SetCityCookie(string city)
        {
            Response.Cookies.Append(
                CityCookieKey,
                city,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
        }

        private static LocationsDto ToDto(GeoCityResult city, string source)
            => new()
            {
                City = city.City,
                State = city.State,
                Country = city.Country,
                Source = source
            };
    }
}