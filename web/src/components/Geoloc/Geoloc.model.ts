export interface Trip {
  TripId: string;
  TripDescription: string;
  Departure: Point;
  Arrival: Point;
  StopPoints: StopPoint[];
  DrivePoints: DrivePoint[];
}

export interface StopPoint extends Point {
  StartDatetime: string;
  DurationInSecond: number;
  HasEngineOff: boolean;
  EndDateTime: string;
}

export interface DrivePoint extends Point {
  LocalTime: string;
  Heading: number;
  Speed: number;
  TotalOdometer: number;
}

export interface Point {
  Latitude: number;
  Longitude: number;
  Label?: string;
}

export interface Poi {
  timestamp: string;
  label: string;
}
