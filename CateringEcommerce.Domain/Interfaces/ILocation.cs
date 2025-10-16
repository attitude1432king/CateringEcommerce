using CateringEcommerce.Domain.Models;

namespace CateringEcommerce.Domain.Interfaces
{
    public interface ILocation
    {
        List<State> GetStates();

        List<City> GetCities(int stateId);
    }
}
