import { describe, expect, it } from "vitest";
import { currentUserLabel } from "./vehicleUserLabel";

const company = { id: "c1", name: "Pompes Sud", isActive: true };

describe("currentUserLabel", () => {
  it("indique le locataire, le propriétaire ou l'absence d'information", () => {
    expect(currentUserLabel({ currentUser: { company, source: "Tenant" } })).toBe("Pompes Sud (locataire)");
    expect(currentUserLabel({ currentUser: { company, source: "Owner" } })).toBe("Pompes Sud");
    expect(currentUserLabel({ currentUser: { company: null, source: "NotSet" } })).toBe("non renseigné");
  });
});
