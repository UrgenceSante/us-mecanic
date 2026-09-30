import { useQuery } from "@tanstack/react-query";
import { getNature } from "../../services/ReferenceData.service";

export default function useGetNature() {
  const request = useQuery({
    queryKey: ["referencedData", "nature"],
    queryFn: getNature,
  });
  return request;
}
