using FluentAssertions;
using SkyRoute.Application.DTOs;
using SkyRoute.Application.Validators;
using SkyRoute.Domain.Enums;
using Xunit;

namespace SkyRoute.Tests.Validators;

public class FlightSearchRequestDtoValidatorTests
{
    private readonly FlightSearchRequestDtoValidator _validator = new();

    private static FlightSearchRequestDto ValidRequest() => new()
    {
        Origin = "EZE",
        Destination = "MIA",
        DepartureDate = DateTime.Today.AddDays(7),
        Passengers = 2,
        CabinClass = CabinClass.Economy
    };

    [Fact]
    public void Valid_request_passes()
    {
        var result = _validator.Validate(ValidRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_origin_fails()
    {
        var dto = ValidRequest();
        dto.Origin = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Origin");
    }

    [Fact]
    public void Empty_destination_fails()
    {
        var dto = ValidRequest();
        dto.Destination = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Destination");
    }

    [Fact]
    public void Origin_equal_to_destination_fails()
    {
        var dto = ValidRequest();
        dto.Destination = dto.Origin;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage.Contains("different"));
    }

    [Fact]
    public void Departure_date_in_past_fails()
    {
        var dto = ValidRequest();
        dto.DepartureDate = DateTime.Today.AddDays(-1);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DepartureDate");
    }

    [Fact]
    public void Departure_date_today_passes()
    {
        var dto = ValidRequest();
        dto.DepartureDate = DateTime.Today;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(-1)]
    public void Passengers_out_of_range_fails(int passengers)
    {
        var dto = ValidRequest();
        dto.Passengers = passengers;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Passengers");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(9)]
    public void Passengers_in_range_passes(int passengers)
    {
        var dto = ValidRequest();
        dto.Passengers = passengers;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Invalid_cabin_class_fails()
    {
        var dto = ValidRequest();
        dto.CabinClass = (CabinClass)99;

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CabinClass");
    }
}
