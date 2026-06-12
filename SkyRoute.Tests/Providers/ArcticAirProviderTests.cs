using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SkyRoute.Domain.Enums;
using SkyRoute.Infrastructure.Providers;
using Xunit;

namespace SkyRoute.Tests.Providers;

public class ArcticAirProviderTests
{
    private readonly ArcticAirProvider _provider =
        new(NullLogger<ArcticAirProvider>.Instance);

    private static readonly DateTime FutureDate = DateTime.Today.AddDays(30);

    [Fact]
    public async Task Returns_between_10_and_12_flights()
    {
        var flights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Count.Should().BeInRange(10, 12);
    }

    [Fact]
    public async Task Same_search_always_returns_same_results()
    {
        var first = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();
        var second = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        first.Select(f => f.FlightNumber).Should().BeEquivalentTo(second.Select(f => f.FlightNumber));
        first.Select(f => f.PricePerPassenger).Should().BeEquivalentTo(second.Select(f => f.PricePerPassenger));
    }

    [Fact]
    public async Task Results_differ_from_other_providers()
    {
        var globalAir = new GlobalAirProvider(NullLogger<GlobalAirProvider>.Instance);
        var budgetWings = new BudgetWingsProvider(NullLogger<BudgetWingsProvider>.Instance);

        var arctic = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy))
            .Select(f => f.FlightNumber).ToList();
        var global = (await globalAir.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy))
            .Select(f => f.FlightNumber).ToList();
        var budget = (await budgetWings.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy))
            .Select(f => f.FlightNumber).ToList();

        arctic.Should().NotBeEquivalentTo(global, because: "each provider uses a different seed");
        arctic.Should().NotBeEquivalentTo(budget, because: "each provider uses a different seed");
    }

    [Fact]
    public async Task All_prices_are_at_least_minimum_floor()
    {
        const decimal minimumPrice = 49.99m;

        var flights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Should().AllSatisfy(f =>
            f.PricePerPassenger.Should().BeGreaterThanOrEqualTo(minimumPrice));
    }

    [Fact]
    public async Task Domestic_flights_have_short_duration()
    {
        var flights = (await _provider.SearchAsync("EZE", "AEP", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Should().AllSatisfy(f =>
            f.Duration.TotalHours.Should().BeLessThan(3,
                because: "EZE→AEP is a domestic route"));
    }

    [Fact]
    public async Task International_flights_have_long_duration()
    {
        var flights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Should().AllSatisfy(f =>
            f.Duration.TotalHours.Should().BeGreaterThan(7,
                because: "EZE→MIA is long-haul"));
    }

    [Fact]
    public async Task Flights_are_ordered_by_departure_time()
    {
        var flights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Should().BeInAscendingOrder(f => f.DepartureTime);
    }

    [Fact]
    public async Task Arrival_time_is_after_departure_time()
    {
        var flights = (await _provider.SearchAsync("GRU", "JFK", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Should().AllSatisfy(f =>
            f.ArrivalTime.Should().BeAfter(f.DepartureTime));
    }

    [Fact]
    public async Task Business_class_costs_more_than_economy()
    {
        var economy = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy))
            .Average(f => f.PricePerPassenger);
        var business = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Business))
            .Average(f => f.PricePerPassenger);

        business.Should().BeGreaterThan(economy);
    }

    [Fact]
    public async Task First_class_costs_more_than_business()
    {
        var business = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Business))
            .Average(f => f.PricePerPassenger);
        var firstClass = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.FirstClass))
            .Average(f => f.PricePerPassenger);

        firstClass.Should().BeGreaterThan(business,
            because: "FirstClass multiplier (4.8x) is higher than Business (2.6x)");
    }

    [Fact]
    public async Task Provider_name_is_ArcticAir()
    {
        _provider.ProviderName.Should().Be("ArcticAir");
    }

    [Fact]
    public async Task No_two_flights_share_the_same_departure_hour()
    {
        var flights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        var hours = flights.Select(f => f.DepartureTime.Hour).ToList();
        hours.Should().OnlyHaveUniqueItems(because: "no two flights depart within the same clock hour");
    }
}
