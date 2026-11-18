import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { Rental } from "../types/rental";

/** « du 01/02/2026 au 31/03/2026 » ou « depuis le 01/02/2026 » sans date de fin. */
export function rentalPeriod(rental: Rental): string {
  return rental.endDate
    ? `du ${formatDateOnly(rental.startDate)} au ${formatDateOnly(rental.endDate)}`
    : `depuis le ${formatDateOnly(rental.startDate)}`;
}
