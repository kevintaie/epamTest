import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet, RouterLink } from '@angular/router';
import { FlightResults } from './components/flight-results/flight-results';
import { FlightBooking } from './components/flight-booking/flight-booking';
import { FlightStateService } from './services/flight-state.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, CommonModule, FlightResults, FlightBooking],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {
  private stateService = inject(FlightStateService);
  selectedFlight$ = this.stateService.selectedFlight$;

  closeDrawer() {
    this.stateService.clearSelection();
  }
}
