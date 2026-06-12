using SkyRoute.Domain.Enums;

namespace SkyRoute.Application.DTOs;

public class FlightSearchRequestDto
{
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime DepartureDate { get; set; }
    public int Passengers { get; set; }
    public CabinClass CabinClass { get; set; }
}
