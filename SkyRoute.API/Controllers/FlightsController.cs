using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SkyRoute.Application.DTOs;
using SkyRoute.Application.Interfaces;

namespace SkyRoute.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightsController : ControllerBase
{
    private readonly IFlightService _flightService;
    private readonly IValidator<FlightSearchRequestDto> _validator;

    public FlightsController(IFlightService flightService, IValidator<FlightSearchRequestDto> validator)
    {
        _flightService = flightService;
        _validator = validator;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] FlightSearchRequestDto request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var results = await _flightService.SearchFlightsAsync(request);
        return Ok(results);
    }
}
