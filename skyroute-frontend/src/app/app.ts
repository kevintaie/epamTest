import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription, combineLatest } from 'rxjs';
import { FlightSearch } from './components/flight-search/flight-search';
import { FlightResults } from './components/flight-results/flight-results';
import { FlightBooking } from './components/flight-booking/flight-booking';
import { FlightStateService } from './services/flight-state.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, FlightSearch, FlightResults, FlightBooking],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App implements OnInit, OnDestroy {
  private stateService = inject(FlightStateService);
  private subscription = new Subscription();

  selectedFlight$ = this.stateService.selectedFlight$;
  showResultsLayout = false;

  ngOnInit() {
    this.subscription.add(
      combineLatest([
        this.stateService.searchPerformed$,
        this.stateService.loading$,
      ]).subscribe(([performed, loading]) => {
        this.showResultsLayout = performed || loading;
      }),
    );
  }

  ngOnDestroy() {
    this.subscription.unsubscribe();
  }

  goHome() {
    this.stateService.resetSearch();
  }

  closeDrawer() {
    this.stateService.clearSelection();
  }
}
