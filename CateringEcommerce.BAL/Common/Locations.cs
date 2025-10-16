using CateringEcommerce.BAL.Configuration;
using CateringEcommerce.BAL.DatabaseHelper;
using CateringEcommerce.Domain.Interfaces;
using CateringEcommerce.Domain.Models;
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

        public List<State> GetStates()
        {
            List<State> states = new List<State>();
            string sqlState = "SELECT c_stateid AS StateID,c_statename AS StateName FROM " + Table.State;

            // Get all states
            var stateDataTable = _db.Execute(sqlState) as DataTable;
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

        public List<City> GetCities(int stateId)
        {
            List<City> cities = new List<City>();
            string sqlCity = $"SELECT c_cityid AS CityID, c_cityname AS CityName FROM {Table.City} WHERE c_stateid = @StateID";
            var parameters = new[]
            {
                new SqlParameter("@StateID", stateId) // Use Microsoft.Data.SqlClient.SqlParameter
            };

            // Use pattern matching to simplify the type check and cast
            if (_db.Execute(sqlCity, parameters) is DataTable cityDataTable)
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
    }
}
