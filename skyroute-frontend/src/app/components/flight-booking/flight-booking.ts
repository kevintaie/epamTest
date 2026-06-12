import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { FlightService } from '../../services/flight.service';
import { FlightStateService } from '../../services/flight-state.service';
import {
  FlightResult,
  FlightSearchRequest,
  AIRPORTS,
  CABIN_CLASS_LABELS,
  BookingRequest,
} from '../../models/flight.model';

@Component({
  selector: 'app-flight-booking',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './flight-booking.html',
  styleUrl: './flight-booking.scss',
})
export class FlightBooking implements OnInit, OnDestroy {
  readonly cabinLabels = CABIN_CLASS_LABELS;

  flight: FlightResult | null = null;
  lastSearch: FlightSearchRequest | null = null;
  bookingForm!: FormGroup;
  isInternational = false;
  bookingReference: string | null = null;
  submitting = false;

  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private flightService: FlightService,
    private stateService: FlightStateService,
  ) {}

  ngOnInit() {
    this.bookingForm = this.fb.group({
      passengerName: ['', [Validators.required, Validators.minLength(2)]],
      email: ['', [Validators.required, Validators.email]],
      documentNumber: ['', Validators.required],
    });

    this.subscriptions.add(
      this.stateService.selectedFlight$.subscribe((flight) => {
        this.flight = flight;
        if (!flight) {
          this.router.navigate(['/']);
          return;
        }
        this.updateDocumentValidation(flight);
      }),
    );

    this.subscriptions.add(
      this.stateService.lastSearch$.subscribe(
        (search) => (this.lastSearch = search),
      ),
    );
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  get documentLabel(): string {
    return this.isInternational ? 'Passport Number' : 'National ID';
  }

  get documentPlaceholder(): string {
    return this.isInternational
      ? 'Enter passport number (e.g. AB1234567)'
      : 'Enter national ID number';
  }

  private updateDocumentValidation(flight: FlightResult) {
    const originAirport = AIRPORTS.find((a) => a.code === flight.origin);
    const destAirport = AIRPORTS.find((a) => a.code === flight.destination);
    this.isInternational = originAirport?.country !== destAirport?.country;

    const docControl = this.bookingForm.get('documentNumber');
    if (this.isInternational) {
      docControl?.setValidators([
        Validators.required,
        Validators.pattern(/^[A-Za-z0-9]+$/),
      ]);
    } else {
      docControl?.setValidators([
        Validators.required,
        Validators.pattern(/^[0-9]+$/),
      ]);
    }
    docControl?.updateValueAndValidity();
  }

  onConfirm() {
    if (this.bookingForm.invalid || !this.flight) return;

    this.submitting = true;
    const formValue = this.bookingForm.value;

    const request: BookingRequest = {
      flightNumber: this.flight.flightNumber,
      providerName: this.flight.providerName,
      origin: this.flight.origin,
      destination: this.flight.destination,
      totalPrice: this.flight.totalPrice,
      passengerName: formValue.passengerName,
      email: formValue.email,
      documentType: this.isInternational ? 'Passport Number' : 'National ID',
      documentNumber: formValue.documentNumber,
    };

    this.flightService.createBooking(request).subscribe({
      next: (response) => {
        this.bookingReference = response.bookingReferenceCode;
        this.submitting = false;
      },
      error: () => {
        this.submitting = false;
      },
    });
  }

  goBack() {
    this.router.navigate(['/']);
  }
}
