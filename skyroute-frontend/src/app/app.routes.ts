import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./components/flight-search/flight-search').then(
        (m) => m.FlightSearch,
      ),
  },
  {
    path: 'booking',
    loadComponent: () =>
      import('./components/flight-booking/flight-booking').then(
        (m) => m.FlightBooking,
      ),
  },
];
