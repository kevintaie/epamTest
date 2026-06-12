import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  FlightSearchRequest,
  FlightResult,
  BookingRequest,
  BookingResponse,
} from '../models/flight.model';

@Injectable({ providedIn: 'root' })
export class FlightService {
  private readonly apiUrl = 'http://localhost:5000/api';

  constructor(private http: HttpClient) {}

  searchFlights(request: FlightSearchRequest): Observable<FlightResult[]> {
    const params = new HttpParams()
      .set('origin', request.origin)
      .set('destination', request.destination)
      .set('departureDate', request.departureDate)
      .set('passengers', request.passengers.toString())
      .set('cabinClass', request.cabinClass.toString());

    return this.http.get<FlightResult[]>(`${this.apiUrl}/flights/search`, {
      params,
    });
  }

  createBooking(request: BookingRequest): Observable<BookingResponse> {
    return this.http.post<BookingResponse>(`${this.apiUrl}/bookings`, request);
  }
}
