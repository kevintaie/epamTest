using Microsoft.Extensions.Logging;
using SkyRoute.Application.DTOs;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.Application.Services;

public class BookingService : IBookingService
{
    private readonly ILogger<BookingService> _logger;

    public BookingService(ILogger<BookingService> logger)
    {
        _logger = logger;
    }

    public Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request)
    {
        var referenceCode = $"SKY-{Random.Shared.Next(1000, 9999)}";
        var passengerNames = string.Join(", ", request.Passengers.Select(p => p.PassengerName));

        _logger.LogInformation(
            "Booking created: {ReferenceCode} for flight {FlightNumber} ({Provider}), {Count} passenger(s): {Passengers}",
            referenceCode, request.FlightNumber, request.ProviderName,
            request.Passengers.Count, passengerNames);

        return Task.FromResult(new BookingResponseDto { BookingReferenceCode = referenceCode });
    }
}
