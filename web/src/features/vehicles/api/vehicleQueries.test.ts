import { describe, expect, it } from "vitest";
import { vehicleKeys } from "./vehicleQueries";

describe("vehicleKeys.list", () => {
  it("contient tous les paramètres du filtre", () => {
    expect(
      vehicleKeys.list({
        search: "ab",
        status: "Active",
        type: "t1",
        ownerCompanyId: "c1",
        ownerMissing: true,
        currentUserCompanyId: "c2",
      }),
    ).toEqual(["vehicles", "list", "ab", "Active", "t1", "c1", true, "c2"]);
  });

  it("distingue le filtre « À renseigner » de l'absence de filtre", () => {
    expect(vehicleKeys.list({ ownerMissing: true })).not.toEqual(vehicleKeys.list({}));
  });
});
