using SkyRoute.Domain.Enums;

namespace SkyRoute.Domain.Entities;

public class Flight
{
    public string Airline { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public TimeSpan Duration { get; set; }
    public decimal PricePerPassenger { get; set; }
    public CabinClass CabinClass { get; set; }
}
