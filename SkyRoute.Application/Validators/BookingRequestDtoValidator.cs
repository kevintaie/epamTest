using FluentValidation;
using SkyRoute.Application.DTOs;

namespace SkyRoute.Application.Validators;

public class BookingRequestDtoValidator : AbstractValidator<BookingRequestDto>
{
    private static readonly Dictionary<string, string> AirportCountryMap = new()
    {
        { "EZE", "AR" },
        { "AEP", "AR" },
        { "GRU", "BR" },
        { "GIG", "BR" },
        { "MIA", "US" },
        { "JFK", "US" }
    };

    public BookingRequestDtoValidator()
    {
        RuleFor(x => x.FlightNumber)
            .NotEmpty().WithMessage("Flight number is required.");

        RuleFor(x => x.ProviderName)
            .NotEmpty().WithMessage("Provider name is required.");

        RuleFor(x => x.PassengerName)
            .NotEmpty().WithMessage("Passenger name is required.")
            .MinimumLength(2).WithMessage("Passenger name must be at least 2 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.DocumentType)
            .NotEmpty().WithMessage("Document type is required.");

        RuleFor(x => x.DocumentNumber)
            .NotEmpty().WithMessage("Document number is required.");

        RuleFor(x => x)
            .Custom((dto, context) =>
            {
                var originCountry = AirportCountryMap.GetValueOrDefault(dto.Origin.ToUpperInvariant());
                var destCountry = AirportCountryMap.GetValueOrDefault(dto.Destination.ToUpperInvariant());

                if (originCountry == null || destCountry == null)
                    return;

                bool isInternational = originCountry != destCountry;

                if (isInternational)
                {
                    if (!string.Equals(dto.DocumentType, "Passport Number", StringComparison.OrdinalIgnoreCase))
                    {
                        context.AddFailure(nameof(dto.DocumentType),
                            "International flights require 'Passport Number' as document type.");
                    }
                }
                else
                {
                    if (!string.Equals(dto.DocumentType, "National ID", StringComparison.OrdinalIgnoreCase))
                    {
                        context.AddFailure(nameof(dto.DocumentType),
                            "Domestic flights require 'National ID' as document type.");
                    }
                }
            });
    }
}
