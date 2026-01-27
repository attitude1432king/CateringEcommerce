using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces.Common;
using CateringEcommerce.Domain.Models.Common;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CateringEcommerce.BAL.Common
{
    public class Locations : ILocation
    {
        private readonly SqlDatabaseManager _db;
        public Locations(string connectionString)
        {
            _db = new SqlDatabaseManager();
            _db.SetConnectionString(connectionString);
        }

        public async Task<List<State>> GetStates()
        {
            List<State> states = new List<State>();
            string sqlState = "SELECT c_stateid AS StateID,c_statename AS StateName FROM " + Table.State;

            // Get all states
            var stateDataTable = await _db.ExecuteAsync(sqlState) as DataTable;
            if (stateDataTable != null)
            {
                states = stateDataTable.AsEnumerable()
                    .Where(row => row["StateName"] != DBNull.Value)
                    .Select(row => new State
                    {
                        StateID = row.Field<int?>("StateID") ?? 0, // Assuming StateID is nullable, adjust as necessary
                        StateName = row["StateName"].ToString()
                    }).ToList();
            }

            return states;
        }

        public async Task<List<City>> GetCities(int stateId)
        {
            List<City> cities = new List<City>();
            string sqlCity = $"SELECT c_cityid AS CityID, c_cityname AS CityName FROM {Table.City} WHERE c_stateid = @StateID";
            var parameters = new[]
            {
                new SqlParameter("@StateID", stateId) // Use Microsoft.Data.SqlClient.SqlParameter
            };

            // Use pattern matching to simplify the type check and cast
            if (await _db.ExecuteAsync(sqlCity, parameters) is DataTable cityDataTable)
            {
                cities = cityDataTable.AsEnumerable()
                   .Where(row => row["CityName"] != DBNull.Value)
                   .Select(row => new City
                   {
                       CityID = row.Field<int?>("CityID") ?? 0,
                       CityName = row["CityName"].ToString()
                   }).ToList();
            }
            return cities;
        }

        //Get the city id based on city name
        public async Task<int> GetCityID(string cityName)
        {
            try
            {
                if (string.IsNullOrEmpty(cityName))
                    return 0;
                string query = $@"SELECT c_cityid FROM {Table.City} WHERE c_cityname = @CityName";
                var parameters = new[]
                {
                    new SqlParameter("@CityName", cityName) // Use Microsoft.Data.SqlClient.SqlParameter
                };

                var result = await _db.ExecuteScalarAsync(query.ToString(), parameters.ToArray());
                int cityId = result != null ? Convert.ToInt32(result) : 0;
                return cityId;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
