import { useQuery } from "@tanstack/react-query";
import type { IMecanicLog } from "../../../components/MecanicLog/IMecanicLog";
import { getMecanicLogs } from "../../../services/MecanicLog.service";

/** Compare les immatriculations sans tenir compte de la casse, des tirets ni des espaces. */
const normalizePlate = (plate: string | null) => (plate ?? "").replace(/[^a-z0-9]/gi, "").toUpperCase();

export function filterLogsByPlate(logs: IMecanicLog[], licensePlate: string): IMecanicLog[] {
  const target = normalizePlate(licensePlate);
  if (!target) return [];
  return logs.filter((log) => normalizePlate(log.Immatriculation) === target);
}

/**
 * Transitoire : les logs viennent de l'ancienne API (pas de filtre côté serveur),
 * ils seront repris par la nouvelle API en phase 4.
 */
export function useVehicleProblems(licensePlate: string) {
  return useQuery({
    queryKey: ["mecanicLogs", "vehicle", licensePlate],
    queryFn: async () => filterLogsByPlate(await getMecanicLogs(), licensePlate),
    enabled: licensePlate !== "",
  });
}
