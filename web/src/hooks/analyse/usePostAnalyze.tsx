import { useMutation } from "@tanstack/react-query";
import { postAnalyze } from "../../services/Analyse.service";
import { queryClient } from "../../api/queryClient";
import type { IAnalyseForm } from "../../components/Analyse/IAnalyse";


export default function usePostAnalyze(
  mutationFn?: (analyse: IAnalyseForm) => Promise<void>
) {
  const mutation = useMutation({
    mutationKey: ["analyze"],
    mutationFn: mutationFn || postAnalyze,
    onSuccess: () => queryClient.invalidateQueries({ queryKey: ["analyze"] }),
  });
  return mutation;
}
