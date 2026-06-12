using FluentValidation;
using SkyRoute.Application.DTOs;

namespace SkyRoute.Application.Validators;

public class FlightSearchRequestDtoValidator : AbstractValidator<FlightSearchRequestDto>
{
    public FlightSearchRequestDtoValidator()
    {
        RuleFor(x => x.Origin)
            .NotEmpty().WithMessage("Origin is required.");

        RuleFor(x => x.Destination)
            .NotEmpty().WithMessage("Destination is required.");

        RuleFor(x => x.Origin)
            .NotEqual(x => x.Destination)
            .WithMessage("Origin and Destination must be different.");

        RuleFor(x => x.DepartureDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Departure date must be today or in the future.");

        RuleFor(x => x.Passengers)
            .InclusiveBetween(1, 9)
            .WithMessage("Passengers must be between 1 and 9.");

        RuleFor(x => x.CabinClass)
            .IsInEnum().WithMessage("Invalid cabin class.");
    }
}
