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

        _logger.LogInformation(
            "Booking created: {ReferenceCode} for flight {FlightNumber} ({Provider}), passenger: {Passenger}",
            referenceCode, request.FlightNumber, request.ProviderName, request.PassengerName);

        return Task.FromResult(new BookingResponseDto
        {
            BookingReferenceCode = referenceCode
        });
    }
}
