import type { Point } from "./Geoloc.model";

const EARTH_RADIUS_M = 6371000;

const toRad = (value: number) => (value * Math.PI) / 180;

/** Distance orthodromique (formule de haversine) entre deux points, en mètres. */
export function distanceInMeters(p1?: Point, p2?: Point): number | undefined {
  if (!p1 || !p2) return undefined;

  const φ1 = toRad(p1.Latitude);
  const φ2 = toRad(p2.Latitude);
  const Δφ = toRad(p2.Latitude - p1.Latitude);
  const Δλ = toRad(p2.Longitude - p1.Longitude);

  const a =
    Math.sin(Δφ / 2) ** 2 + Math.cos(φ1) * Math.cos(φ2) * Math.sin(Δλ / 2) ** 2;

  return EARTH_RADIUS_M * 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
}
