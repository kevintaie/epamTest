using Microsoft.Extensions.Logging;
using SkyRoute.Application.DTOs;
using SkyRoute.Application.Interfaces;
using SkyRoute.Domain.Interfaces;

namespace SkyRoute.Application.Services;

public class FlightService : IFlightService
{
    private readonly IEnumerable<IFlightProvider> _providers;
    private readonly ILogger<FlightService> _logger;

    public FlightService(IEnumerable<IFlightProvider> providers, ILogger<FlightService> logger)
    {
        _providers = providers;
        _logger = logger;
    }

    public async Task<IEnumerable<FlightResultDto>> SearchFlightsAsync(FlightSearchRequestDto request)
    {
        _logger.LogInformation(
            "Searching flights from {Origin} to {Destination} on {Date} for {Passengers} passenger(s), class {CabinClass}",
            request.Origin, request.Destination, request.DepartureDate, request.Passengers, request.CabinClass);

        var searchTasks = _providers.Select(provider =>
            SearchProviderAsync(provider, request));

        var results = await Task.WhenAll(searchTasks);

        var flights = results
            .SelectMany(r => r)
            .ToList();

        _logger.LogInformation("Found {Count} total flights across all providers", flights.Count);

        return flights;
    }

    private async Task<IEnumerable<FlightResultDto>> SearchProviderAsync(
        IFlightProvider provider, FlightSearchRequestDto request)
    {
        try
        {
            _logger.LogInformation("Querying provider {Provider}...", provider.ProviderName);

            var flights = await provider.SearchAsync(
                request.Origin,
                request.Destination,
                request.DepartureDate,
                request.Passengers,
                request.CabinClass);

            return flights.Select(f => new FlightResultDto
            {
                ProviderName = provider.ProviderName,
                FlightNumber = f.FlightNumber,
                Origin = f.Origin,
                Destination = f.Destination,
                DepartureTime = f.DepartureTime,
                ArrivalTime = f.ArrivalTime,
                Duration = $"{(int)f.Duration.TotalHours}h {f.Duration.Minutes}m",
                CabinClass = f.CabinClass,
                PricePerPassenger = f.PricePerPassenger,
                TotalPrice = f.PricePerPassenger * request.Passengers
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error querying provider {Provider}", provider.ProviderName);
            return [];
        }
    }
}
