using FluentAssertions;
using SkyRoute.Application.DTOs;
using SkyRoute.Application.Validators;
using Xunit;

namespace SkyRoute.Tests.Validators;

public class BookingRequestDtoValidatorTests
{
    private readonly BookingRequestDtoValidator _validator = new();

    private static BookingRequestDto ValidDomesticRequest() => new()
    {
        FlightNumber = "GA-123",
        ProviderName = "GlobalAir",
        Origin = "EZE",
        Destination = "AEP",
        TotalPrice = 150m,
        PassengerName = "John Doe",
        Email = "john@example.com",
        DocumentType = "National ID",
        DocumentNumber = "12345678"
    };

    private static BookingRequestDto ValidInternationalRequest() => new()
    {
        FlightNumber = "GA-456",
        ProviderName = "GlobalAir",
        Origin = "EZE",
        Destination = "MIA",
        TotalPrice = 800m,
        PassengerName = "Jane Doe",
        Email = "jane@example.com",
        DocumentType = "Passport Number",
        DocumentNumber = "AB123456"
    };

    [Fact]
    public void Valid_domestic_booking_passes()
    {
        var result = _validator.Validate(ValidDomesticRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_international_booking_passes()
    {
        var result = _validator.Validate(ValidInternationalRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void International_flight_with_national_id_fails()
    {
        var dto = ValidInternationalRequest();
        dto.DocumentType = "National ID";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "DocumentType" &&
            e.ErrorMessage.Contains("Passport Number"));
    }

    [Fact]
    public void Domestic_flight_with_passport_fails()
    {
        var dto = ValidDomesticRequest();
        dto.DocumentType = "Passport Number";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "DocumentType" &&
            e.ErrorMessage.Contains("National ID"));
    }

    [Fact]
    public void Missing_flight_number_fails()
    {
        var dto = ValidDomesticRequest();
        dto.FlightNumber = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FlightNumber");
    }

    [Fact]
    public void Missing_provider_name_fails()
    {
        var dto = ValidDomesticRequest();
        dto.ProviderName = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProviderName");
    }

    [Fact]
    public void Missing_passenger_name_fails()
    {
        var dto = ValidDomesticRequest();
        dto.PassengerName = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PassengerName");
    }

    [Fact]
    public void Short_passenger_name_fails()
    {
        var dto = ValidDomesticRequest();
        dto.PassengerName = "A";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "PassengerName");
    }

    [Fact]
    public void Missing_email_fails()
    {
        var dto = ValidDomesticRequest();
        dto.Email = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Invalid_email_format_fails()
    {
        var dto = ValidDomesticRequest();
        dto.Email = "not-an-email";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Missing_document_number_fails()
    {
        var dto = ValidDomesticRequest();
        dto.DocumentNumber = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "DocumentNumber");
    }

    [Fact]
    public void Unknown_airport_skips_document_type_country_rule()
    {
        var dto = ValidDomesticRequest();
        dto.Origin = "XXX";
        dto.Destination = "YYY";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeTrue();
    }
}
