using FluentValidation;
using SkyRoute.Application.DTOs;

namespace SkyRoute.Application.Validators;

public class BookingRequestDtoValidator : AbstractValidator<BookingRequestDto>
{
    private static readonly Dictionary<string, string> AirportCountryMap = new()
    {
        { "EZE", "AR" }, { "AEP", "AR" },
        { "GRU", "BR" }, { "GIG", "BR" },
        { "MIA", "US" }, { "JFK", "US" }
    };

    public BookingRequestDtoValidator()
    {
        RuleFor(x => x.FlightNumber)
            .NotEmpty().WithMessage("Flight number is required.");

        RuleFor(x => x.ProviderName)
            .NotEmpty().WithMessage("Provider name is required.");

        RuleFor(x => x.Passengers)
            .NotEmpty().WithMessage("At least one passenger is required.");

        RuleFor(x => x.Passengers)
            .Must(passengers => passengers?.All(passenger => passenger is not null) ?? true)
            .WithMessage("Passenger is required.");

        RuleForEach(x => x.Passengers).ChildRules(passenger =>
        {
            passenger.RuleFor(p => p.PassengerName)
                .NotEmpty().WithMessage("Passenger name is required.")
                .MinimumLength(2).WithMessage("Passenger name must be at least 2 characters.");

            passenger.RuleFor(p => p.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("A valid email address is required.");

            passenger.RuleFor(p => p.DocumentType)
                .NotEmpty().WithMessage("Document type is required.");

            passenger.RuleFor(p => p.DocumentNumber)
                .NotEmpty().WithMessage("Document number is required.");
        });

        RuleFor(x => x).Custom((dto, context) =>
        {
            var originCountry = string.IsNullOrWhiteSpace(dto.Origin)
                ? null
                : AirportCountryMap.GetValueOrDefault(dto.Origin.ToUpperInvariant());
            var destCountry = string.IsNullOrWhiteSpace(dto.Destination)
                ? null
                : AirportCountryMap.GetValueOrDefault(dto.Destination.ToUpperInvariant());

            if (originCountry == null || destCountry == null)
                return;

            if (dto.Passengers is null)
                return;

            bool isInternational = originCountry != destCountry;
            string required = isInternational ? "Passport Number" : "National ID";
            string message = isInternational
                ? "International flights require 'Passport Number' as document type."
                : "Domestic flights require 'National ID' as document type.";

            for (int i = 0; i < dto.Passengers.Count; i++)
            {
                var passenger = dto.Passengers[i];
                if (passenger is null)
                    continue;

                if (!string.Equals(passenger.DocumentType, required, StringComparison.OrdinalIgnoreCase))
                    context.AddFailure($"Passengers[{i}].DocumentType", message);
            }
        });
    }
}
