import AsyncSelect from "./AsyncSelect";
import useGetActors from "../../../../hooks/referencedData/useGetActors";
import type { SelectProps } from "@mui/material";

export default function ActorSelect(props: SelectProps<string>) {
  const req = useGetActors();

  return <AsyncSelect selectProps={{ ...props, label: "Acteur" }} req={req} />;
}
