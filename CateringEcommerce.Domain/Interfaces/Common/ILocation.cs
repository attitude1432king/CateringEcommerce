using CateringEcommerce.Domain.Models.Common;

namespace CateringEcommerce.Domain.Interfaces.Common
{
    public interface ILocation
    {
        List<State> GetStates();

        List<City> GetCities(int stateId);
    }
}
