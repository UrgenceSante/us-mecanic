import { useQuery } from "@tanstack/react-query";
import { getConstraints } from "../../services/ReferenceData.service";

export default function useGetConstraints() {
  const request = useQuery({
    queryKey: ["referencedData", "constraints"],
    queryFn: getConstraints,
  });
  return request;
}
