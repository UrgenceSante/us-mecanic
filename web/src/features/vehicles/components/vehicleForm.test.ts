import { describe, expect, it } from "vitest";
import { formatDateOnly, toFormValues, toRegisterCommand } from "./vehicleForm";
import type { Vehicle } from "../types/vehicle";

describe("vehicleForm", () => {
  it("convertit les champs vides en null et nettoie les espaces", () => {
    const command = toRegisterCommand({
      licensePlate: " ab-123-cd ",
      brand: " Renault ",
      model: "",
      firstRegistrationDate: "2021-03-15",
      vin: "  ",
      internalNumber: "AMB-1",
    });

    expect(command).toEqual({
      licensePlate: "ab-123-cd",
      details: {
        brand: "Renault",
        model: null,
        firstRegistrationDate: "2021-03-15",
        vin: null,
        internalNumber: "AMB-1",
      },
    });
  });

  it("préremplit le formulaire depuis un véhicule", () => {
    const vehicle: Vehicle = {
      id: "1",
      licensePlate: "AB-123-CD",
      brand: null,
      model: "Master",
      firstRegistrationDate: null,
      vin: null,
      internalNumber: null,
      status: "Active",
      createdAt: "",
      updatedAt: "",
    };

    expect(toFormValues(vehicle)).toMatchObject({ licensePlate: "AB-123-CD", brand: "", model: "Master" });
  });

  it("formate une date sans décalage de fuseau", () => {
    expect(formatDateOnly("2021-03-01")).toBe("01/03/2021");
    expect(formatDateOnly(null)).toBe("-");
  });
});
