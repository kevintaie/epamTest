import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  Validators,
} from '@angular/forms';
import { Subscription, finalize } from 'rxjs';
import { FlightService } from '../../services/flight.service';
import { FlightStateService } from '../../services/flight-state.service';
import {
  AIRPORTS,
  CabinClass,
  CABIN_CLASS_LABELS,
  FlightSearchRequest,
} from '../../models/flight.model';

@Component({
  selector: 'app-flight-search',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './flight-search.html',
  styleUrl: './flight-search.scss',
})
export class FlightSearch {
  @Input() compact = false;

  readonly airports = AIRPORTS;
  readonly cabinClasses = [
    { value: CabinClass.Economy, label: CABIN_CLASS_LABELS[CabinClass.Economy] },
    { value: CabinClass.Business, label: CABIN_CLASS_LABELS[CabinClass.Business] },
    { value: CabinClass.FirstClass, label: CABIN_CLASS_LABELS[CabinClass.FirstClass] },
  ];
  readonly passengerOptions = Array.from({ length: 9 }, (_, i) => i + 1);
  readonly minDate: string;

  searchForm: FormGroup;
  submitting = false;

  constructor(
    private fb: FormBuilder,
    private flightService: FlightService,
    private stateService: FlightStateService,
  ) {
    const today = new Date();
    this.minDate = today.toISOString().split('T')[0];

    this.searchForm = this.fb.group({
      origin: ['', Validators.required],
      destination: ['', Validators.required],
      departureDate: [this.minDate, Validators.required],
      passengers: [1, [Validators.required, Validators.min(1), Validators.max(9)]],
      cabinClass: [CabinClass.Economy, Validators.required],
    });
  }

  get availableDestinations() {
    const origin = this.searchForm.get('origin')?.value;
    return this.airports.filter((a) => a.code !== origin);
  }

  onSearch() {
    if (this.searchForm.invalid || this.submitting) return;

    const formValue = this.searchForm.value;
    const request: FlightSearchRequest = {
      origin: formValue.origin,
      destination: formValue.destination,
      departureDate: formValue.departureDate,
      passengers: formValue.passengers,
      cabinClass: formValue.cabinClass,
    };

    this.submitting = true;
    this.stateService.setLoading(true);
    this.stateService.setLastSearch(request);
    this.stateService.clearSelection();

    this.flightService
      .searchFlights(request)
      .pipe(finalize(() => {
        this.submitting = false;
        this.stateService.setLoading(false);
      }))
      .subscribe({
        next: (results) => {
          this.stateService.setResults(results);
        },
        error: () => {
          this.stateService.setResults([]);
        },
      });
  }
}
