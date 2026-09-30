import { describe, expect, it } from "vitest";
import { distanceInMeters } from "./geo.utils";

describe("distanceInMeters", () => {
  it("retourne undefined si un point manque", () => {
    expect(distanceInMeters(undefined, { Latitude: 0, Longitude: 0 })).toBeUndefined();
  });

  it("retourne 0 pour deux points identiques", () => {
    const p = { Latitude: 48.8566, Longitude: 2.3522 };
    expect(distanceInMeters(p, p)).toBe(0);
  });

  it("calcule la distance Paris - Lyon (~392 km)", () => {
    const paris = { Latitude: 48.8566, Longitude: 2.3522 };
    const lyon = { Latitude: 45.764, Longitude: 4.8357 };
    const km = distanceInMeters(paris, lyon)! / 1000;
    expect(km).toBeGreaterThan(390);
    expect(km).toBeLessThan(394);
  });
});
