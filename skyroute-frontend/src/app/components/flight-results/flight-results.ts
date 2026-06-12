import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import {
  FlightStateService,
  SortField,
  SortDirection,
} from '../../services/flight-state.service';
import { FlightResult, CABIN_CLASS_LABELS } from '../../models/flight.model';

@Component({
  selector: 'app-flight-results',
  imports: [CommonModule],
  templateUrl: './flight-results.html',
  styleUrl: './flight-results.scss',
})
export class FlightResults {
  private stateService = inject(FlightStateService);
  private router = inject(Router);

  readonly cabinLabels = CABIN_CLASS_LABELS;

  results$ = this.stateService.results$;
  loading$ = this.stateService.loading$;
  searchPerformed$ = this.stateService.searchPerformed$;

  currentSort: SortField | null = null;
  currentDirection: SortDirection = 'asc';

  sort(field: SortField) {
    if (this.currentSort === field) {
      this.currentDirection = this.currentDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.currentSort = field;
      this.currentDirection = 'asc';
    }
    this.stateService.sortResults(field, this.currentDirection);
  }

  getSortIcon(field: SortField): string {
    if (this.currentSort !== field) return '↕';
    return this.currentDirection === 'asc' ? '↑' : '↓';
  }

  selectFlight(flight: FlightResult) {
    this.stateService.selectFlight(flight);
    this.router.navigate(['/booking']);
  }
}
