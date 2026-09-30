import { useQuery } from "@tanstack/react-query";
import { getMecanicLogs } from "../../services/MecanicLog.service";

export default function useGetMecanicLogs() {
  const request = useQuery({
    queryKey: ["mecanicLogs"],
    queryFn: getMecanicLogs,
  });

  return request;
}
