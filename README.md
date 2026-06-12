# SkyRoute Travel Platform

A full-stack Flight Search & Booking application built with **.NET 10** and **Angular 21**, developed as part of the EPAM Senior Full-Stack Engineering Assessment.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Backend | .NET 10 (C#), ASP.NET Core Web API |
| Frontend | Angular 21, TypeScript, SCSS |
| Validation | FluentValidation |
| Architecture | Clean Architecture / Modular Monolith |

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 22+](https://nodejs.org/)
- Angular CLI (`npm install -g @angular/cli`)

---

## Running Locally

### 1. Backend API

```bash
cd SkyRoute.API
dotnet run
```

The API starts at `http://localhost:5000`.

### 2. Frontend

```bash
cd skyroute-frontend
npm install
ng serve
```

The app starts at `http://localhost:4200`.

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/flights/search` | Search flights across all providers |
| `POST` | `/api/bookings` | Create a booking and receive a reference code |

### Flight Search Query Parameters

| Parameter | Type | Example |
|---|---|---|
| `origin` | string | `EZE` |
| `destination` | string | `MIA` |
| `departureDate` | ISO date | `2026-07-20` |
| `passengers` | int (1–9) | `2` |
| `cabinClass` | int (0=Economy, 1=Business, 2=FirstClass) | `0` |

---

## Architecture

The backend follows **Clean Architecture** with four layers, each with a strict dependency rule (inner layers know nothing about outer ones):

```
SkyRoute.Domain          → Entities, Enums, IFlightProvider interface
SkyRoute.Application     → Service interfaces, DTOs, FluentValidation validators, FlightService
SkyRoute.Infrastructure  → Mock provider implementations (GlobalAir, BudgetWings)
SkyRoute.API             → Controllers, ExceptionMiddleware, DI wiring (Program.cs)
```

### Key Design Decisions

**Strategy Pattern for flight providers**

Each airline provider implements `IFlightProvider`. The `FlightService` receives `IEnumerable<IFlightProvider>` via dependency injection and queries all of them in parallel using `Task.WhenAll`. Adding a new provider only requires creating a class that implements the interface and registering it in `Program.cs` — no changes to existing code.

**Service-oriented (no CQRS / MediatR)**

The application layer uses plain injected services (`IFlightService`, `IBookingService`). CQRS was deliberately avoided — the scope is small enough that the added indirection would be noise rather than value.

**Global Exception Middleware (RFC 7807)**

All unhandled exceptions are caught by `ExceptionMiddleware`. `FluentValidation.ValidationException` maps to `400 Bad Request` with a structured error list. All other exceptions map to `500 Internal Server Error`. Every response uses the RFC 7807 `ProblemDetails` format.

**Realistic mock providers**

Both `GlobalAirProvider` and `BudgetWingsProvider` generate flight data dynamically using a seeded `Random` (seeded by route + date), so:
- Results are **deterministic** — the same search always returns the same flights.
- Results are **route-aware** — domestic flights have short durations (~1–2h), international flights have realistic long-haul durations.
- Results are **date-aware** — last-minute searches (≤3 days out) cost more; booking far ahead costs less.

Provider pricing rules are applied inside each provider before returning flights:

| Provider | Pricing Rule |
|---|---|
| GlobalAir | `baseFare × 1.15` (fuel surcharge), always rounded to 2 decimal places |
| BudgetWings | `baseFare × 0.90` (promotional discount), minimum final price $29.99 |

**Frontend state management**

A `FlightStateService` backed by `BehaviorSubject` stores search results after the first API call. Sorting (price, duration, departure) re-orders the in-memory array without triggering a new HTTP request. `FlightResultsComponent` subscribes to the observable and re-renders reactively.

**Dynamic document validation**

The booking form detects whether the selected route is international (origin and destination in different countries) by mapping airport codes to country codes. If international, the document field is labelled *Passport Number* and accepts alphanumeric input. If domestic, it is labelled *National ID* and accepts digits only. The same rule is enforced on the backend via a `BookingRequestDtoValidator` custom rule.

---

## Project Structure

```
EPAM/
├── SkyRoute.Domain/
│   ├── Entities/           Flight, Booking
│   ├── Enums/              CabinClass
│   └── Interfaces/         IFlightProvider
│
├── SkyRoute.Application/
│   ├── DTOs/               FlightSearchRequestDto, FlightResultDto, BookingRequestDto, BookingResponseDto
│   ├── Interfaces/         IFlightService, IBookingService
│   ├── Services/           FlightService, BookingService
│   └── Validators/         FlightSearchRequestDtoValidator, BookingRequestDtoValidator
│
├── SkyRoute.Infrastructure/
│   └── Providers/          GlobalAirProvider, BudgetWingsProvider
│
├── SkyRoute.API/
│   ├── Controllers/        FlightsController, BookingsController
│   ├── Middleware/         ExceptionMiddleware
│   └── Program.cs
│
└── skyroute-frontend/
    └── src/app/
        ├── models/         flight.model.ts
        ├── services/       FlightService, FlightStateService
        └── components/     FlightSearch, FlightResults, FlightBooking
```

---

## Airports (Hardcoded)

| Code | Airport | Country |
|---|---|---|
| EZE | Buenos Aires – Ezeiza | Argentina |
| AEP | Buenos Aires – Aeroparque | Argentina |
| GRU | São Paulo – Guarulhos | Brazil |
| GIG | Rio de Janeiro – Galeão | Brazil |
| MIA | Miami International | United States |
| JFK | New York – JFK | United States |

---

## Known Limitations & Trade-offs

- **No persistence** — Bookings are generated in memory. A reference code is returned but there is no endpoint to retrieve or cancel a booking. In a production system, bookings would be persisted to a database (e.g., SQL Server via EF Core).
- **No authentication** — The API is open. Production would require JWT-based auth.
- **Mock providers only** — `GlobalAirProvider` and `BudgetWingsProvider` return synthesised data. Integration with real airline APIs would require HTTP clients, retry policies (Polly), and circuit breakers.
- **No unit/integration tests** — Out of scope for the time constraint. The architecture is fully testable: providers are injected interfaces, services are thin, and validators are isolated.
- **`PricePerPassenger` in the `Flight` entity holds the post-pricing-rule price** — The raw base fare used internally by each provider is not exposed on the domain entity. For a production system, both values would be tracked to support price auditing and margin calculations.
