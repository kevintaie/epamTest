using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SkyRoute.Application.DTOs;
using SkyRoute.Application.Services;
using SkyRoute.Domain.Entities;
using SkyRoute.Domain.Enums;
using SkyRoute.Domain.Interfaces;
using Xunit;

namespace SkyRoute.Tests.Services;

public class FlightServiceTests
{
    private static FlightSearchRequestDto SearchRequest(int passengers = 2) => new()
    {
        Origin = "EZE",
        Destination = "MIA",
        DepartureDate = DateTime.Today.AddDays(10),
        Passengers = passengers,
        CabinClass = CabinClass.Economy
    };

    private static Flight MakeFlight(string airline, decimal price) => new()
    {
        Airline = airline,
        FlightNumber = $"{airline}-001",
        Origin = "EZE",
        Destination = "MIA",
        DepartureTime = DateTime.Today.AddDays(10).AddHours(8),
        ArrivalTime = DateTime.Today.AddDays(10).AddHours(20),
        Duration = TimeSpan.FromHours(12),
        PricePerPassenger = price,
        CabinClass = CabinClass.Economy
    };

    [Fact]
    public async Task SearchFlightsAsync_aggregates_results_from_all_providers()
    {
        var providerA = new Mock<IFlightProvider>();
        providerA.Setup(p => p.ProviderName).Returns("ProviderA");
        providerA.Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CabinClass>()))
            .ReturnsAsync(new[] { MakeFlight("AA", 300m) });

        var providerB = new Mock<IFlightProvider>();
        providerB.Setup(p => p.ProviderName).Returns("ProviderB");
        providerB.Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CabinClass>()))
            .ReturnsAsync(new[] { MakeFlight("BB", 200m), MakeFlight("BB", 250m) });

        var service = new FlightService(
            new[] { providerA.Object, providerB.Object },
            NullLogger<FlightService>.Instance);

        var results = (await service.SearchFlightsAsync(SearchRequest())).ToList();

        results.Should().HaveCount(3);
    }

    [Fact]
    public async Task TotalPrice_equals_price_per_passenger_times_passengers()
    {
        const decimal price = 400m;
        const int passengers = 3;

        var provider = new Mock<IFlightProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CabinClass>()))
            .ReturnsAsync(new[] { MakeFlight("TP", price) });

        var service = new FlightService(
            new[] { provider.Object },
            NullLogger<FlightService>.Instance);

        var results = (await service.SearchFlightsAsync(SearchRequest(passengers))).ToList();

        results.Single().TotalPrice.Should().Be(price * passengers);
        results.Single().PricePerPassenger.Should().Be(price);
    }

    [Fact]
    public async Task Duration_is_formatted_as_hours_and_minutes()
    {
        var flight = MakeFlight("TP", 300m);
        flight.Duration = new TimeSpan(9, 45, 0);

        var provider = new Mock<IFlightProvider>();
        provider.Setup(p => p.ProviderName).Returns("TestProvider");
        provider.Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CabinClass>()))
            .ReturnsAsync(new[] { flight });

        var service = new FlightService(
            new[] { provider.Object },
            NullLogger<FlightService>.Instance);

        var result = (await service.SearchFlightsAsync(SearchRequest())).Single();

        result.Duration.Should().Be("9h 45m");
    }

    [Fact]
    public async Task Provider_error_is_swallowed_and_other_providers_still_return_results()
    {
        var faultyProvider = new Mock<IFlightProvider>();
        faultyProvider.Setup(p => p.ProviderName).Returns("FaultyProvider");
        faultyProvider.Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CabinClass>()))
            .ThrowsAsync(new HttpRequestException("Upstream timeout"));

        var goodProvider = new Mock<IFlightProvider>();
        goodProvider.Setup(p => p.ProviderName).Returns("GoodProvider");
        goodProvider.Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CabinClass>()))
            .ReturnsAsync(new[] { MakeFlight("GP", 350m) });

        var service = new FlightService(
            new[] { faultyProvider.Object, goodProvider.Object },
            NullLogger<FlightService>.Instance);

        var act = async () => await service.SearchFlightsAsync(SearchRequest());

        await act.Should().NotThrowAsync();
        var results = (await service.SearchFlightsAsync(SearchRequest())).ToList();
        results.Should().HaveCount(1);
    }

    [Fact]
    public async Task ProviderName_is_mapped_from_provider()
    {
        var provider = new Mock<IFlightProvider>();
        provider.Setup(p => p.ProviderName).Returns("MyAirline");
        provider.Setup(p => p.SearchAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<DateTime>(), It.IsAny<int>(), It.IsAny<CabinClass>()))
            .ReturnsAsync(new[] { MakeFlight("MA", 300m) });

        var service = new FlightService(
            new[] { provider.Object },
            NullLogger<FlightService>.Instance);

        var result = (await service.SearchFlightsAsync(SearchRequest())).Single();

        result.ProviderName.Should().Be("MyAirline");
    }
}
