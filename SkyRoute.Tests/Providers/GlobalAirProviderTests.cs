using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SkyRoute.Domain.Enums;
using SkyRoute.Infrastructure.Providers;
using Xunit;

namespace SkyRoute.Tests.Providers;

public class GlobalAirProviderTests
{
    private readonly GlobalAirProvider _provider =
        new(NullLogger<GlobalAirProvider>.Instance);

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
    public async Task Different_routes_return_different_results()
    {
        var ezeToMia = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();
        var ezeToGru = (await _provider.SearchAsync("EZE", "GRU", FutureDate, 1, CabinClass.Economy)).ToList();

        ezeToMia.Select(f => f.FlightNumber).Should()
            .NotBeEquivalentTo(ezeToGru.Select(f => f.FlightNumber));
    }

    [Fact]
    public async Task Domestic_flights_have_short_duration()
    {
        var flights = (await _provider.SearchAsync("EZE", "AEP", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Should().AllSatisfy(f =>
            f.Duration.TotalHours.Should().BeLessThan(3,
                because: "EZE→AEP is a short domestic route"));
    }

    [Fact]
    public async Task International_flights_have_long_duration()
    {
        var flights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Should().AllSatisfy(f =>
            f.Duration.TotalHours.Should().BeGreaterThan(7,
                because: "EZE→MIA is a long-haul international route"));
    }

    [Fact]
    public async Task All_flights_have_correct_origin_and_destination()
    {
        var flights = (await _provider.SearchAsync("EZE", "JFK", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Should().AllSatisfy(f =>
        {
            f.Origin.Should().Be("EZE");
            f.Destination.Should().Be("JFK");
        });
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
        var flights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

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
    public async Task Provider_name_is_GlobalAir()
    {
        _provider.ProviderName.Should().Be("GlobalAir");
    }

    [Fact]
    public async Task Prices_are_positive()
    {
        var flights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        flights.Should().AllSatisfy(f =>
            f.PricePerPassenger.Should().BePositive());
    }

    [Fact]
    public async Task No_two_flights_share_the_same_departure_hour()
    {
        var flights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        var hours = flights.Select(f => f.DepartureTime.Hour).ToList();
        hours.Should().OnlyHaveUniqueItems(because: "no two flights depart within the same clock hour");
    }

    [Fact]
    public async Task Combined_with_budget_wings_returns_at_least_20_flights()
    {
        var budgetWings = new BudgetWingsProvider(NullLogger<BudgetWingsProvider>.Instance);

        var globalFlights = (await _provider.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();
        var budgetFlights = (await budgetWings.SearchAsync("EZE", "MIA", FutureDate, 1, CabinClass.Economy)).ToList();

        (globalFlights.Count + budgetFlights.Count).Should().BeGreaterThanOrEqualTo(20);
    }
}
