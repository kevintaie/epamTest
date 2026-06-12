import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
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
export class FlightResults implements OnInit, OnDestroy {
  private stateService = inject(FlightStateService);
  private subscription = new Subscription();

  readonly cabinLabels = CABIN_CLASS_LABELS;

  loading$ = this.stateService.loading$;
  searchPerformed$ = this.stateService.searchPerformed$;

  allResults: FlightResult[] = [];
  paginatedResults: FlightResult[] = [];
  currentPage = 1;
  pageSize = 5;
  pageSizeOptions = [5, 10, 20];

  currentSort: SortField | null = null;
  currentDirection: SortDirection = 'asc';

  ngOnInit() {
    this.subscription.add(
      this.stateService.results$.subscribe(results => {
        this.allResults = results;
        this.currentPage = 1;
        this.applyPagination();
      }),
    );
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  get totalPages() {
    return Math.ceil(this.allResults.length / this.pageSize) || 1;
  }

  get pages(): number[] {
    const maxVisible = 5;
    const total = this.totalPages;
    if (total <= maxVisible) return Array.from({ length: total }, (_, i) => i + 1);

    const half = Math.floor(maxVisible / 2);
    let start = Math.max(this.currentPage - half, 1);
    let end = start + maxVisible - 1;

    if (end > total) {
      end = total;
      start = Math.max(end - maxVisible + 1, 1);
    }

    return Array.from({ length: end - start + 1 }, (_, i) => start + i);
  }

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
    if (this.currentSort !== field) return '\u2195';
    return this.currentDirection === 'asc' ? '\u2191' : '\u2193';
  }

  selectFlight(flight: FlightResult) {
    this.stateService.selectFlight(flight);
  }

  goToPage(page: number) {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
    this.applyPagination();
  }

  setPageSize(size: number) {
    this.pageSize = size;
    this.currentPage = 1;
    this.applyPagination();
  }

  private applyPagination() {
    const start = (this.currentPage - 1) * this.pageSize;
    this.paginatedResults = this.allResults.slice(start, start + this.pageSize);
  }
}
