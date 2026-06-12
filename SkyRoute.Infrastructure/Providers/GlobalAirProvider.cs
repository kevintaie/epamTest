using Microsoft.Extensions.Logging;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Interfaces;

namespace SkyRoute.Infrastructure.Providers;

public class GlobalAirProvider : IFlightProvider
{
    private readonly ILogger<GlobalAirProvider> _logger;

    public string ProviderName => "GlobalAir";

    private static readonly Dictionary<string, string> AirportCountry = new()
    {
        { "EZE", "AR" }, { "AEP", "AR" },
        { "GRU", "BR" }, { "GIG", "BR" },
        { "MIA", "US" }, { "JFK", "US" }
    };

    private static readonly Dictionary<(string, string), (int MinHours, int MaxHours)> RouteDurations = new()
    {
        { ("AR", "AR"), (1, 2) },
        { ("BR", "BR"), (1, 2) },
        { ("US", "US"), (2, 4) },
        { ("AR", "BR"), (2, 4) },
        { ("BR", "AR"), (2, 4) },
        { ("AR", "US"), (9, 12) },
        { ("US", "AR"), (9, 12) },
        { ("BR", "US"), (8, 11) },
        { ("US", "BR"), (8, 11) }
    };

    public GlobalAirProvider(ILogger<GlobalAirProvider> logger)
    {
        _logger = logger;
    }

    public Task<IEnumerable<Flight>> SearchAsync(string origin, string destination, DateTime date, int passengers, CabinClass cabin)
    {
        _logger.LogInformation("GlobalAir searching {Origin} -> {Destination} on {Date}", origin, destination, date);

        var seed = HashCode.Combine(origin, destination, date.DayOfYear, date.Year);
        var rng = new Random(seed);

        var originCountry = AirportCountry.GetValueOrDefault(origin, "XX");
        var destCountry = AirportCountry.GetValueOrDefault(destination, "XX");
        var (minH, maxH) = RouteDurations.GetValueOrDefault((originCountry, destCountry), (3, 6));

        var baseEconomy = GetRouteBasePrice(originCountry, destCountry, rng);
        var dateFactor = GetDatePriceFactor(date);
        var cabinMultiplier = cabin switch
        {
            CabinClass.Business => 2.8m,
            CabinClass.FirstClass => 5.5m,
            _ => 1m
        };

        var flightCount = rng.Next(2, 5);
        var flights = new List<Flight>();
        var usedHours = new HashSet<int>();

        for (int i = 0; i < flightCount; i++)
        {
            int departureHour;
            do { departureHour = rng.Next(5, 23); }
            while (usedHours.Any(h => Math.Abs(h - departureHour) < 2));
            usedHours.Add(departureHour);

            var durationMinutes = rng.Next(minH * 60, maxH * 60 + 1);
            var duration = TimeSpan.FromMinutes(durationMinutes);
            var departureTime = date.AddHours(departureHour).AddMinutes(rng.Next(0, 4) * 15);
            var arrivalTime = departureTime.Add(duration);

            var priceSeed = baseEconomy * cabinMultiplier * dateFactor;
            var variation = 0.9m + (decimal)rng.NextDouble() * 0.25m;
            var finalPrice = ApplyFuelSurcharge(priceSeed * variation);

            flights.Add(new Flight
            {
                Airline = "GlobalAir",
                FlightNumber = $"GA-{rng.Next(100, 999)}",
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
        var baseMin = isDomestic ? 80m : 250m;
        var baseMax = isDomestic ? 200m : 650m;
        return baseMin + (decimal)rng.NextDouble() * (baseMax - baseMin);
    }

    private static decimal GetDatePriceFactor(DateTime date)
    {
        var daysOut = (date - DateTime.Today).TotalDays;
        if (daysOut <= 3) return 1.6m;
        if (daysOut <= 14) return 1.25m;
        if (daysOut <= 60) return 1.0m;
        return 0.9m;
    }

    private static decimal ApplyFuelSurcharge(decimal baseFare)
    {
        return Math.Round(baseFare * 1.15m, 2);
    }
}
