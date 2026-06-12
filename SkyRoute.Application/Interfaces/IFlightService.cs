using SkyRoute.Application.DTOs;

namespace SkyRoute.Application.Interfaces;

public interface IFlightService
{
    Task<IEnumerable<FlightResultDto>> SearchFlightsAsync(FlightSearchRequestDto request);
}
