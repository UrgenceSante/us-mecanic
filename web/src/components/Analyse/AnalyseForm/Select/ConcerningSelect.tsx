import type { SelectProps } from "@mui/material";
import useGetConcerning from "../../../../hooks/referencedData/useGetConcerning";
import AsyncSelect from "./AsyncSelect";

export default function ConcerningSelect(props: SelectProps<string>) {
  const req = useGetConcerning();

  return (
    <AsyncSelect selectProps={{ ...props, label: "Concerne" }} req={req} />
  );
}
