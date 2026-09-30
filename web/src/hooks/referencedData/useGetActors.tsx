import { useQuery } from "@tanstack/react-query";
import { getActors } from "../../services/ReferenceData.service";

export default function useGetActors() {
  const request = useQuery({
    queryKey: ["referencedData", "actors"],
    queryFn: getActors,
  });
  return request;
}
