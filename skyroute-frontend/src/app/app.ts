import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { FlightResults } from './components/flight-results/flight-results';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, FlightResults],
  templateUrl: './app.html',
  styleUrl: './app.scss',
})
export class App {}
