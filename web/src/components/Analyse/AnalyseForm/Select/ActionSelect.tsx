import AsyncSelect from "./AsyncSelect";
import useGetAction from "../../../../hooks/referencedData/useGetAction";
import type { SelectProps } from "@mui/material";

export default function ActionSelect(props: SelectProps<string>) {
  const req = useGetAction();

  return <AsyncSelect selectProps={{ ...props, label: "Action" }} req={req} />;
}
