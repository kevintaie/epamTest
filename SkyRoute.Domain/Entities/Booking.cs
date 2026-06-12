namespace SkyRoute.Domain.Entities;

public class Booking
{
    public string ReferenceCode { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public string PassengerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
}
