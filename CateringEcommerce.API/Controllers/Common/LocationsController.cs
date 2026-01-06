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
        private readonly IConfiguration _configuration;
        private readonly string _connStr;

        private const string CityCookieKey = "user_city";

        private readonly IGeoLocationService _geoService;
        private readonly IMemoryCache _cache;
        public LocationsController(IConfiguration configuration, IGeoLocationService geoService, IMemoryCache cache)
        {
            _configuration = configuration;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
            _geoService = geoService;
            _cache = cache;
        }

        [AllowAnonymous]
        [HttpGet("states")]
        public async Task<IActionResult> GetStates()
        {
            try
            {
                var states = new List<State>();
                Locations locations = new Locations(_connStr);
                states = await locations.GetStates();
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
                var cities = new List<City>();
                Locations locations = new Locations(_connStr);
                cities = await locations.GetCities(stateId);
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