import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { vehicleKeys } from "../../vehicles/api/vehicleQueries";
import type { RentalInput } from "../types/rental";
import { addRental, deleteRental, endRental, listRentals, updateRental } from "./rentalsApi";

export const rentalKeys = {
  list: (vehicleId: string) => ["rentals", vehicleId] as const,
};

export function useRentals(vehicleId: string) {
  return useQuery({ queryKey: rentalKeys.list(vehicleId), queryFn: () => listRentals(vehicleId), enabled: vehicleId !== "" });
}

/** Une écriture invalide aussi le véhicule : l'utilisateur actuel (currentUser) en dépend. */
function useInvalidateRentals(vehicleId: string) {
  const queryClient = useQueryClient();
  return () =>
    Promise.all([
      queryClient.invalidateQueries({ queryKey: rentalKeys.list(vehicleId) }),
      queryClient.invalidateQueries({ queryKey: vehicleKeys.all }),
    ]);
}

export function useAddRental(vehicleId: string) {
  const onSuccess = useInvalidateRentals(vehicleId);
  return useMutation({ mutationFn: (input: RentalInput) => addRental(vehicleId, input), onSuccess });
}

export function useUpdateRental(vehicleId: string) {
  const onSuccess = useInvalidateRentals(vehicleId);
  return useMutation({
    mutationFn: ({ rentalId, input }: { rentalId: string; input: RentalInput }) => updateRental(vehicleId, rentalId, input),
    onSuccess,
  });
}

export function useEndRental(vehicleId: string) {
  const onSuccess = useInvalidateRentals(vehicleId);
  return useMutation({
    mutationFn: ({ rentalId, endDate }: { rentalId: string; endDate: string }) => endRental(vehicleId, rentalId, endDate),
    onSuccess,
  });
}

export function useDeleteRental(vehicleId: string) {
  const onSuccess = useInvalidateRentals(vehicleId);
  return useMutation({ mutationFn: (rentalId: string) => deleteRental(vehicleId, rentalId), onSuccess });
}
