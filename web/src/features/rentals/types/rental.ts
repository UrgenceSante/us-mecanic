import type { Company } from "../../companies/types/company";

export type RentalState = "Planned" | "Ongoing" | "Ended";

export interface Rental {
  id: string;
  vehicleId: string;
  tenant: Company;
  /** Date ISO AAAA-MM-JJ. */
  startDate: string;
  /** Date ISO AAAA-MM-JJ, null pour une location sans terme connu. */
  endDate: string | null;
  /** État calculé par l'API à la date du jour. */
  state: RentalState;
}

export interface RentalInput {
  tenantCompanyId: string;
  startDate: string;
  endDate: string | null;
}

export const rentalStateLabels: Record<RentalState, string> = {
  Planned: "Prévue",
  Ongoing: "En cours",
  Ended: "Terminée",
};

/** Historique : de la plus récente à la plus ancienne (date de début). */
export function sortRentals(rentals: Rental[]): Rental[] {
  return [...rentals].sort((a, b) => b.startDate.localeCompare(a.startDate));
}

/** Location en cours (au plus une : les périodes ne se chevauchent pas). */
export function ongoingRental(rentals: Rental[] | undefined): Rental | undefined {
  return rentals?.find((r) => r.state === "Ongoing");
}
