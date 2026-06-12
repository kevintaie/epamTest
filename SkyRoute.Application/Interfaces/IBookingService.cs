using SkyRoute.Application.DTOs;

namespace SkyRoute.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto request);
}
