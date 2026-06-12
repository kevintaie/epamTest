export enum CabinClass {
  Economy = 0,
  Business = 1,
  FirstClass = 2,
}

export const CABIN_CLASS_LABELS: Record<CabinClass, string> = {
  [CabinClass.Economy]: 'Economy',
  [CabinClass.Business]: 'Business',
  [CabinClass.FirstClass]: 'First Class',
};

export interface FlightSearchRequest {
  origin: string;
  destination: string;
  departureDate: string;
  passengers: number;
  cabinClass: CabinClass;
}

export interface FlightResult {
  providerName: string;
  flightNumber: string;
  origin: string;
  destination: string;
  departureTime: string;
  arrivalTime: string;
  duration: string;
  cabinClass: CabinClass;
  totalPrice: number;
  pricePerPassenger: number;
}

export interface PassengerInfo {
  passengerName: string;
  email: string;
  documentType: string;
  documentNumber: string;
}

export interface BookingRequest {
  flightNumber: string;
  providerName: string;
  origin: string;
  destination: string;
  totalPrice: number;
  passengers: PassengerInfo[];
}

export interface BookingResponse {
  bookingReferenceCode: string;
}

export interface Airport {
  code: string;
  name: string;
  country: string;
}

export const AIRPORTS: Airport[] = [
  { code: 'EZE', name: 'Buenos Aires - Ezeiza', country: 'AR' },
  { code: 'AEP', name: 'Buenos Aires - Aeroparque', country: 'AR' },
  { code: 'GRU', name: 'São Paulo - Guarulhos', country: 'BR' },
  { code: 'GIG', name: 'Rio de Janeiro - Galeão', country: 'BR' },
  { code: 'MIA', name: 'Miami International', country: 'US' },
  { code: 'JFK', name: 'New York - JFK', country: 'US' },
];
