import { describe, expect, it } from "vitest";
import { formatParisDate } from "./formatParisDate";

describe("formatParisDate", () => {
  it("passe au jour suivant après 22h UTC en été (heure de Paris)", () => {
    expect(formatParisDate("2026-07-10T22:30:00Z")).toBe("11/07/2026");
  });
  it("garde le même jour en milieu de journée", () => {
    expect(formatParisDate("2026-03-10T08:00:00Z")).toBe("10/03/2026");
  });
});
