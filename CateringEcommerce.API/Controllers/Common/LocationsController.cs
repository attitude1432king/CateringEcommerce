using CateringEcommerce.BAL.Common;
using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using Twilio.TwiML.Voice;

namespace CateringEcommerce.API.Controllers.Common
{
    [ApiController]
    [Route("api/Common/Locations")]
    public class LocationsController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _connStr;

        public LocationsController(IConfiguration configuration)
        {
            _configuration = configuration;
            _connStr = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("DefaultConnection string is not configured.");
        }

        [Authorize]
        [HttpGet("states")]
        public IActionResult GetStates()
        {
            try
            {
                var states = new List<State>();
                Locations locations = new Locations(_connStr);
                states = locations.GetStates();
                return Ok(states);
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching states.", ex); // Internal Server Error
            }
        }

        [Authorize]
        [HttpGet("cities/{stateId}")]
        public IActionResult GetCities(int stateId)
        {
            try
            {
                var cities = new List<City>();
                Locations locations = new Locations(_connStr);
                cities = locations.GetCities(stateId);
                return Ok(cities);
            }
            catch (Exception ex) 
            {
                throw new Exception($"An error occurred while fetching cities for state '{stateId}'.", ex); // Internal Server Error
            }

        }
    }
}