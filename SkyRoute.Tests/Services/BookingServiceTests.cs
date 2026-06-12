using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using SkyRoute.Application.DTOs;
using SkyRoute.Application.Services;
using System.Text.RegularExpressions;
using Xunit;

namespace SkyRoute.Tests.Services;

public class BookingServiceTests
{
    private readonly BookingService _service = new(NullLogger<BookingService>.Instance);

    private static BookingRequestDto SampleRequest() => new()
    {
        FlightNumber = "GA-200",
        ProviderName = "GlobalAir",
        Origin = "EZE",
        Destination = "MIA",
        TotalPrice = 900m,
        PassengerName = "Maria García",
        Email = "maria@example.com",
        DocumentType = "Passport Number",
        DocumentNumber = "AB987654"
    };

    [Fact]
    public async Task CreateBookingAsync_returns_response_with_reference_code()
    {
        var response = await _service.CreateBookingAsync(SampleRequest());

        response.Should().NotBeNull();
        response.BookingReferenceCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Reference_code_starts_with_SKY_prefix()
    {
        var response = await _service.CreateBookingAsync(SampleRequest());

        response.BookingReferenceCode.Should().StartWith("SKY-");
    }

    [Fact]
    public async Task Reference_code_matches_expected_format()
    {
        var response = await _service.CreateBookingAsync(SampleRequest());

        Regex.IsMatch(response.BookingReferenceCode, @"^SKY-\d{4}$")
            .Should().BeTrue(because: $"'{response.BookingReferenceCode}' should match SKY-XXXX");
    }

    [Fact]
    public async Task Multiple_bookings_can_produce_different_codes()
    {
        var codes = new HashSet<string>();
        for (int i = 0; i < 20; i++)
        {
            var response = await _service.CreateBookingAsync(SampleRequest());
            codes.Add(response.BookingReferenceCode);
        }

        codes.Count.Should().BeGreaterThan(1, because: "random codes should vary across calls");
    }
}
