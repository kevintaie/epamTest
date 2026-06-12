using Microsoft.Extensions.Logging;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Interfaces;

namespace SkyRoute.Infrastructure.Providers;

public class ArcticAirProvider : IFlightProvider
{
    private readonly ILogger<ArcticAirProvider> _logger;

    public string ProviderName => "ArcticAir";

    private static readonly Dictionary<string, string> AirportCountry = new()
    {
        { "EZE", "AR" }, { "AEP", "AR" },
        { "GRU", "BR" }, { "GIG", "BR" },
        { "MIA", "US" }, { "JFK", "US" }
    };

    private static readonly Dictionary<(string, string), (int MinHours, int MaxHours)> RouteDurations = new()
    {
        { ("AR", "AR"), (1, 2) },
        { ("BR", "BR"), (1, 3) },
        { ("US", "US"), (3, 5) },
        { ("AR", "BR"), (3, 5) },
        { ("BR", "AR"), (3, 5) },
        { ("AR", "US"), (9, 12) },
        { ("US", "AR"), (9, 12) },
        { ("BR", "US"), (8, 11) },
        { ("US", "BR"), (8, 11) }
    };

    public ArcticAirProvider(ILogger<ArcticAirProvider> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<Flight>> SearchAsync(string origin, string destination, DateTime date, int passengers, CabinClass cabin)
    {
        _logger.LogInformation("ArcticAir searching {Origin} -> {Destination} on {Date}", origin, destination, date);

        var seed = HashCode.Combine("AA", origin, destination, date.DayOfYear, date.Year);
        var rng = new Random(seed);

        var originCountry = AirportCountry.GetValueOrDefault(origin, "XX");
        var destCountry = AirportCountry.GetValueOrDefault(destination, "XX");
        var (minH, maxH) = RouteDurations.GetValueOrDefault((originCountry, destCountry), (4, 7));

        var baseEconomy = GetRouteBasePrice(originCountry, destCountry, rng);
        var dateFactor = GetDatePriceFactor(date);
        var cabinMultiplier = cabin switch
        {
            CabinClass.Business => 2.6m,
            CabinClass.FirstClass => 4.8m,
            _ => 1m
        };

        var flightCount = rng.Next(10, 13);
        var flights = new List<Flight>();
        var usedHours = new HashSet<int>();

        for (int i = 0; i < flightCount; i++)
        {
            int departureHour;
            do { departureHour = rng.Next(4, 24); }
            while (usedHours.Contains(departureHour));
            usedHours.Add(departureHour);

            var durationMinutes = rng.Next(minH * 60, maxH * 60 + 1);
            var duration = TimeSpan.FromMinutes(durationMinutes);
            var departureTime = date.AddHours(departureHour).AddMinutes(rng.Next(0, 4) * 15);
            var arrivalTime = departureTime.Add(duration);

            var priceSeed = baseEconomy * cabinMultiplier * dateFactor;
            var variation = 0.88m + (decimal)rng.NextDouble() * 0.24m;
            var finalPrice = ApplyLoyaltyPricing(priceSeed * variation);

            flights.Add(new Flight
            {
                Airline = "ArcticAir",
                FlightNumber = $"AA-{rng.Next(100, 599)}",
                Origin = origin,
                Destination = destination,
                DepartureTime = departureTime,
                ArrivalTime = arrivalTime,
                Duration = duration,
                PricePerPassenger = finalPrice,
                CabinClass = cabin
            });
        }

        return Task.FromResult<IEnumerable<Flight>>(flights.OrderBy(f => f.DepartureTime));
    }

    private static decimal GetRouteBasePrice(string originCountry, string destCountry, Random rng)
    {
        var isDomestic = originCountry == destCountry;
        var baseMin = isDomestic ? 65m : 200m;
        var baseMax = isDomestic ? 180m : 550m;
        return baseMin + (decimal)rng.NextDouble() * (baseMax - baseMin);
    }

    private static decimal GetDatePriceFactor(DateTime date)
    {
        var daysOut = (date - DateTime.Today).TotalDays;
        if (daysOut <= 3) return 1.55m;
        if (daysOut <= 14) return 1.2m;
        if (daysOut <= 60) return 1.0m;
        return 0.88m;
    }

    private static decimal ApplyLoyaltyPricing(decimal baseFare)
    {
        var adjusted = baseFare * 1.2m - 10m;
        return Math.Max(Math.Round(adjusted, 2), 49.99m);
    }
}
