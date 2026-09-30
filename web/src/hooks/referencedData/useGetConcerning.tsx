import { useQuery } from "@tanstack/react-query";
import { getConcerning } from "../../services/ReferenceData.service";

export default function useGetConcerning() {
  const request = useQuery({
    queryKey: ["referencedData", "concerning"],
    queryFn: getConcerning,
  });
  return request;
}
