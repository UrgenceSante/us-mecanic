import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import type { LoanInput } from "../types/loan";
import { loanKeys } from "./loanKeys";
import { addLoan, deleteLoan, listLoans, updateLoan } from "./loansApi";

export function useLoans(vehicleId: string) {
  return useQuery({ queryKey: loanKeys.list(vehicleId), queryFn: () => listLoans(vehicleId), enabled: vehicleId !== "" });
}

function useInvalidateLoans(vehicleId: string) {
  const queryClient = useQueryClient();
  return () => queryClient.invalidateQueries({ queryKey: loanKeys.list(vehicleId) });
}

export function useAddLoan(vehicleId: string) {
  const onSuccess = useInvalidateLoans(vehicleId);
  return useMutation({ mutationFn: (input: LoanInput) => addLoan(vehicleId, input), onSuccess });
}

export function useUpdateLoan(vehicleId: string) {
  const onSuccess = useInvalidateLoans(vehicleId);
  return useMutation({
    mutationFn: ({ loanId, input }: { loanId: string; input: LoanInput }) => updateLoan(vehicleId, loanId, input),
    onSuccess,
  });
}

export function useDeleteLoan(vehicleId: string) {
  const onSuccess = useInvalidateLoans(vehicleId);
  return useMutation({ mutationFn: (loanId: string) => deleteLoan(vehicleId, loanId), onSuccess });
}
