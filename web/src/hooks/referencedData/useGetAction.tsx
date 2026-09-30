import { useQuery } from "@tanstack/react-query";
import { getActions } from "../../services/ReferenceData.service";

export default function useGetAction() {
  const request = useQuery({
    queryKey: ["referencedData", "actions"],
    queryFn: getActions,
  });
  return request;
}
