import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { FlightResult, FlightSearchRequest } from '../models/flight.model';

export type SortField = 'price' | 'duration' | 'departure';
export type SortDirection = 'asc' | 'desc';

@Injectable({ providedIn: 'root' })
export class FlightStateService {
  private readonly resultsSubject = new BehaviorSubject<FlightResult[]>([]);
  private readonly loadingSubject = new BehaviorSubject<boolean>(false);
  private readonly searchPerformedSubject = new BehaviorSubject<boolean>(false);
  private readonly lastSearchSubject =
    new BehaviorSubject<FlightSearchRequest | null>(null);
  private readonly selectedFlightSubject =
    new BehaviorSubject<FlightResult | null>(null);

  readonly results$ = this.resultsSubject.asObservable();
  readonly loading$ = this.loadingSubject.asObservable();
  readonly searchPerformed$ = this.searchPerformedSubject.asObservable();
  readonly lastSearch$ = this.lastSearchSubject.asObservable();
  readonly selectedFlight$ = this.selectedFlightSubject.asObservable();

  setResults(results: FlightResult[]) {
    this.resultsSubject.next(results);
    this.searchPerformedSubject.next(true);
  }

  setLoading(loading: boolean) {
    this.loadingSubject.next(loading);
  }

  setLastSearch(search: FlightSearchRequest) {
    this.lastSearchSubject.next(search);
  }

  selectFlight(flight: FlightResult) {
    this.selectedFlightSubject.next(flight);
  }

  clearSelection() {
    this.selectedFlightSubject.next(null);
  }

  resetSearch() {
    this.resultsSubject.next([]);
    this.loadingSubject.next(false);
    this.searchPerformedSubject.next(false);
    this.lastSearchSubject.next(null);
    this.selectedFlightSubject.next(null);
  }

  sortResults(field: SortField, direction: SortDirection) {
    const current = [...this.resultsSubject.value];
    const multiplier = direction === 'asc' ? 1 : -1;

    current.sort((a, b) => {
      switch (field) {
        case 'price':
          return (a.totalPrice - b.totalPrice) * multiplier;
        case 'duration':
          return (
            (this.parseDuration(a.duration) - this.parseDuration(b.duration)) *
            multiplier
          );
        case 'departure':
          return (
            (new Date(a.departureTime).getTime() -
              new Date(b.departureTime).getTime()) *
            multiplier
          );
      }
    });

    this.resultsSubject.next(current);
  }

  private parseDuration(duration: string): number {
    const match = duration.match(/(\d+)h\s*(\d+)m/);
    if (!match) return 0;
    return parseInt(match[1]) * 60 + parseInt(match[2]);
  }
}
