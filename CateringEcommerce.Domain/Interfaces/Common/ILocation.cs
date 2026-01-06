using CateringEcommerce.Domain.Models.Common;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface ILocation
    {
        Task<List<State>> GetStates();

        Task<List<City>> GetCities(int stateId);

        Task<int> GetCityID(string cityName);
    }
}
