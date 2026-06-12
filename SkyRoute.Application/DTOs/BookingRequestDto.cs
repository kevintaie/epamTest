namespace SkyRoute.Application.DTOs;

public class BookingRequestDto
{
    public string FlightNumber { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
}
