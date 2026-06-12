import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
} from '@angular/forms';
import { Subscription, combineLatest, finalize } from 'rxjs';
import { FlightService } from '../../services/flight.service';
import { FlightStateService } from '../../services/flight-state.service';
import {
  FlightResult,
  FlightSearchRequest,
  AIRPORTS,
  CABIN_CLASS_LABELS,
  BookingRequest,
  PassengerInfo,
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
  activeTab = 0;
  bookingReference: string | null = null;
  submitting = false;

  private subscriptions = new Subscription();

  constructor(
    private fb: FormBuilder,
    private flightService: FlightService,
    private stateService: FlightStateService,
    private cdr: ChangeDetectorRef,
  ) {}

  ngOnInit() {
    this.bookingForm = this.fb.group({ passengers: this.fb.array([]) });

    this.subscriptions.add(
      combineLatest([
        this.stateService.selectedFlight$,
        this.stateService.lastSearch$,
      ]).subscribe(([flight, search]) => {
        this.flight = flight;
        this.lastSearch = search;
        if (flight) {
          this.buildPassengerForms(search?.passengers ?? 1, flight);
        }
      }),
    );
  }

  ngOnDestroy() {
    this.subscriptions.unsubscribe();
  }

  get passengerForms(): FormArray {
    return this.bookingForm.get('passengers') as FormArray;
  }

  get documentLabel(): string {
    return this.isInternational ? 'Passport Number' : 'National ID';
  }

  get documentPlaceholder(): string {
    return this.isInternational
      ? 'Enter passport number (e.g. AB1234567)'
      : 'Enter national ID number';
  }

  setTab(index: number) {
    this.activeTab = index;
  }

  isTabValid(index: number): boolean {
    return this.passengerForms.at(index).valid;
  }

  isTabTouched(index: number): boolean {
    return this.passengerForms.at(index).touched;
  }

  passengerLabel(index: number): string {
    const name = this.passengerForms.at(index).value?.passengerName?.trim();
    return name ? name : `Passenger ${index + 1}`;
  }

  private buildPassengerForms(count: number, flight: FlightResult) {
    const originAirport = AIRPORTS.find((a) => a.code === flight.origin);
    const destAirport = AIRPORTS.find((a) => a.code === flight.destination);
    this.isInternational = originAirport?.country !== destAirport?.country;

    const docValidators = this.isInternational
      ? [Validators.required, Validators.pattern(/^[A-Za-z0-9]+$/)]
      : [Validators.required, Validators.pattern(/^[0-9]+$/)];

    const fa = this.passengerForms;
    fa.clear();
    for (let i = 0; i < count; i++) {
      fa.push(
        this.fb.group({
          passengerName: ['', [Validators.required, Validators.minLength(2)]],
          email: ['', [Validators.required, Validators.email]],
          documentNumber: ['', docValidators],
        }),
      );
    }
    this.activeTab = 0;
    this.submitting = false;
  }

  onConfirm() {
    if (!this.flight) return;

    this.bookingForm.markAllAsTouched();
    if (this.bookingForm.invalid) {
      const firstInvalid = this.passengerForms.controls.findIndex(
        (c) => c.invalid,
      );
      if (firstInvalid >= 0) this.activeTab = firstInvalid;
      return;
    }

    this.bookingReference = null;
    this.submitting = true;
    const docType = this.isInternational ? 'Passport Number' : 'National ID';

    const passengers: PassengerInfo[] = this.passengerForms.controls.map((ctrl) => ({
      passengerName: ctrl.value.passengerName,
      email: ctrl.value.email,
      documentType: docType,
      documentNumber: ctrl.value.documentNumber,
    }));

    const request: BookingRequest = {
      flightNumber: this.flight!.flightNumber,
      providerName: this.flight!.providerName,
      origin: this.flight!.origin,
      destination: this.flight!.destination,
      totalPrice: this.flight!.totalPrice,
      passengers,
    };

    this.flightService
      .createBooking(request)
      .pipe(finalize(() => {
        this.submitting = false;
        this.cdr.markForCheck();
      }))
      .subscribe({
        next: (response) => {
          this.bookingReference = response.bookingReferenceCode;
          this.cdr.markForCheck();
        },
        error: () => {
          this.submitting = false;
          this.cdr.markForCheck();
        },
      });
  }

  close() {
    this.bookingReference = null;
    this.submitting = false;
    this.stateService.clearSelection();
  }
}
