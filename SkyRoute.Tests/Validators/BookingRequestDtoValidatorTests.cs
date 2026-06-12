using FluentAssertions;
using SkyRoute.Application.DTOs;
using SkyRoute.Application.Validators;
using Xunit;

namespace SkyRoute.Tests.Validators;

public class BookingRequestDtoValidatorTests
{
    private readonly BookingRequestDtoValidator _validator = new();

    private static PassengerDto DomesticPassenger() => new()
    {
        PassengerName = "John Doe",
        Email = "john@example.com",
        DocumentType = "National ID",
        DocumentNumber = "12345678"
    };

    private static PassengerDto InternationalPassenger() => new()
    {
        PassengerName = "Jane Doe",
        Email = "jane@example.com",
        DocumentType = "Passport Number",
        DocumentNumber = "AB123456"
    };

    private static BookingRequestDto ValidDomesticRequest(int passengerCount = 1) => new()
    {
        FlightNumber = "GA-123",
        ProviderName = "GlobalAir",
        Origin = "EZE",
        Destination = "AEP",
        TotalPrice = 150m,
        Passengers = Enumerable.Range(0, passengerCount).Select(_ => DomesticPassenger()).ToList()
    };

    private static BookingRequestDto ValidInternationalRequest(int passengerCount = 1) => new()
    {
        FlightNumber = "GA-456",
        ProviderName = "GlobalAir",
        Origin = "EZE",
        Destination = "MIA",
        TotalPrice = 800m,
        Passengers = Enumerable.Range(0, passengerCount).Select(_ => InternationalPassenger()).ToList()
    };

    [Fact]
    public void Valid_domestic_single_passenger_passes()
    {
        var result = _validator.Validate(ValidDomesticRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_international_single_passenger_passes()
    {
        var result = _validator.Validate(ValidInternationalRequest());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_domestic_multiple_passengers_passes()
    {
        var result = _validator.Validate(ValidDomesticRequest(3));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Valid_international_multiple_passengers_passes()
    {
        var result = _validator.Validate(ValidInternationalRequest(3));
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_passengers_list_fails()
    {
        var dto = ValidDomesticRequest();
        dto.Passengers.Clear();

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Passengers");
    }

    [Fact]
    public void Null_passenger_entry_fails()
    {
        var dto = ValidDomesticRequest();
        dto.Passengers.Add(null!);

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Passengers");
    }

    [Fact]
    public void International_flight_with_national_id_fails_for_that_passenger()
    {
        var dto = ValidInternationalRequest(2);
        dto.Passengers[1].DocumentType = "National ID";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Passengers[1].DocumentType" &&
            e.ErrorMessage.Contains("Passport Number"));
    }

    [Fact]
    public void Domestic_flight_with_passport_fails_for_that_passenger()
    {
        var dto = ValidDomesticRequest(2);
        dto.Passengers[0].DocumentType = "Passport Number";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "Passengers[0].DocumentType" &&
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
        dto.Passengers[0].PassengerName = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("PassengerName"));
    }

    [Fact]
    public void Short_passenger_name_fails()
    {
        var dto = ValidDomesticRequest();
        dto.Passengers[0].PassengerName = "A";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("PassengerName"));
    }

    [Fact]
    public void Invalid_email_format_fails()
    {
        var dto = ValidDomesticRequest();
        dto.Passengers[0].Email = "not-an-email";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("Email"));
    }

    [Fact]
    public void Missing_document_number_fails()
    {
        var dto = ValidDomesticRequest();
        dto.Passengers[0].DocumentNumber = "";

        var result = _validator.Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("DocumentNumber"));
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
