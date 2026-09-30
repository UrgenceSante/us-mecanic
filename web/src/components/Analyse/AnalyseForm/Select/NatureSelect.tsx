import type { SelectProps } from "@mui/material";
import useGetNature from "../../../../hooks/referencedData/useGetNature";
import AsyncSelect from "./AsyncSelect";

export default function NatureSelect(props: SelectProps<string>) {
  const req = useGetNature();

  return <AsyncSelect selectProps={{ ...props, label: "Nature" }} req={req} />;
}
