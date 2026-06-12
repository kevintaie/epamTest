using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;

namespace SkyRoute.Domain.Interfaces;

public interface IFlightProvider
{
    string ProviderName { get; }
    Task<IEnumerable<Flight>> SearchAsync(string origin, string destination, DateTime date, int passengers, CabinClass cabin);
}
