import type { RegisterVehicleCommand, Vehicle, VehicleDetailsInput } from "../types/vehicle";

/** Valeurs saisies dans le formulaire : chaînes, vides si non renseignées. */
export interface VehicleFormValues {
  licensePlate: string;
  brand: string;
  model: string;
  firstRegistrationDate: string;
  vin: string;
  internalNumber: string;
}

export const emptyVehicleForm: VehicleFormValues = {
  licensePlate: "",
  brand: "",
  model: "",
  firstRegistrationDate: "",
  vin: "",
  internalNumber: "",
};

export function toFormValues(vehicle: Vehicle): VehicleFormValues {
  return {
    licensePlate: vehicle.licensePlate,
    brand: vehicle.brand ?? "",
    model: vehicle.model ?? "",
    firstRegistrationDate: vehicle.firstRegistrationDate ?? "",
    vin: vehicle.vin ?? "",
    internalNumber: vehicle.internalNumber ?? "",
  };
}

const orNull = (value: string) => (value.trim() === "" ? null : value.trim());

export function toDetailsInput(values: VehicleFormValues): VehicleDetailsInput {
  return {
    brand: orNull(values.brand),
    model: orNull(values.model),
    firstRegistrationDate: orNull(values.firstRegistrationDate),
    vin: orNull(values.vin),
    internalNumber: orNull(values.internalNumber),
  };
}

export function toRegisterCommand(values: VehicleFormValues): RegisterVehicleCommand {
  return { licensePlate: values.licensePlate.trim(), details: toDetailsInput(values) };
}

/** Affiche une date ISO AAAA-MM-JJ en JJ/MM/AAAA, sans conversion de fuseau. */
export function formatDateOnly(value: string | null): string {
  if (!value) return "-";
  const [year, month, day] = value.split("-");
  return `${day}/${month}/${year}`;
}
