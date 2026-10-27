import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { vehicleKeys } from "../../vehicles/api/vehicleQueries";
import type { MileageReadingInput } from "../types/mileageReading";
import { addMileageReading, deleteMileageReading, listMileageReadings, updateMileageReading } from "./mileageApi";

export const mileageKeys = {
  all: ["mileage-readings"] as const,
  list: (vehicleId: string) => ["mileage-readings", "list", vehicleId] as const,
};

export function useMileageReadings(vehicleId: string) {
  return useQuery({
    queryKey: mileageKeys.list(vehicleId),
    queryFn: () => listMileageReadings(vehicleId),
    enabled: vehicleId !== "",
  });
}

function useInvalidateMileage(vehicleId: string) {
  const queryClient = useQueryClient();
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: mileageKeys.list(vehicleId) }),
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all }),
    ]);
}

/** Un relevé change le km actuel : on invalide l'historique, la fiche et la liste des véhicules. */
export function useAddMileageReading(vehicleId: string) {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (input: MileageReadingInput) => addMileageReading(vehicleId, input),
    onSuccess: () =>
      Promise.all([
        queryClient.invalidateQueries({ queryKey: mileageKeys.list(vehicleId) }),
        queryClient.invalidateQueries({ queryKey: vehicleKeys.all }),
      ]),
  });
}

export function useUpdateMileageReading(vehicleId: string) {
  const invalidate = useInvalidateMileage(vehicleId);
  return useMutation({
    mutationFn: ({ readingId, input }: { readingId: string; input: MileageReadingInput }) =>
      updateMileageReading(vehicleId, readingId, input),
    onSuccess: invalidate,
  });
}

export function useDeleteMileageReading(vehicleId: string) {
  const invalidate = useInvalidateMileage(vehicleId);
  return useMutation({
    mutationFn: (readingId: string) => deleteMileageReading(vehicleId, readingId),
    onSuccess: invalidate,
  });
}
