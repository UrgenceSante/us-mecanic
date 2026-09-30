import { useQuery } from "@tanstack/react-query";
import { getMecanicLogsById } from "../../services/MecanicLog.service";

export default function useGetMecanicLogById(id: string) {
  const request = useQuery({
    queryKey: ["mecanicLogs", [id]],
    queryFn: () => getMecanicLogsById(parseInt(id)),
  });

  return request;
}
