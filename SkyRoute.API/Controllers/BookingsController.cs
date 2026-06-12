using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.DTOs;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IValidator<BookingRequestDto> _validator;

    public BookingsController(IBookingService bookingService, IValidator<BookingRequestDto> validator)
    {
        _bookingService = bookingService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var result = await _bookingService.CreateBookingAsync(request);
        return Ok(result);
    }
}
