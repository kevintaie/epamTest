using SkyRoute.Domain.Enums;

namespace SkyRoute.Application.DTOs;

public class FlightResultDto
{
    public string ProviderName { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public string Duration { get; set; } = string.Empty;
    public CabinClass CabinClass { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal PricePerPassenger { get; set; }
}
