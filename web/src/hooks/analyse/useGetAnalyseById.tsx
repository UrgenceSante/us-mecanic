import { useQuery } from "@tanstack/react-query";
import { getAnalyseById } from "../../services/Analyse.service";

export default function useGetAnalyseById(id: string) {
  const request = useQuery({
    queryKey: ["analyze", id],
    queryFn: () => getAnalyseById(id),
  });
  return request;
}
