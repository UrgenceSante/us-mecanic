import useGetConstraints from "../../../../hooks/referencedData/useGetConstraints";
import {
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Skeleton,
  type SelectProps,
} from "@mui/material";
import type { IDisplayValue } from "../../IAnalyse";
import ErrorHandler from "../../../Utils/Error/ErrorHandler";

export default function ConstraintSelect(props: SelectProps<string>) {
  const req = useGetConstraints();

  if (req.isLoading) return <Skeleton width={150} height={70} />;
  if (req.isError) return <ErrorHandler error={req.error} />;

  return (
    <FormControl sx={{ minWidth: 150, flex: 1 }} size="small">
      <InputLabel id="action-select-label">Echéance</InputLabel>
      <Select sx={{ flex: 1 }} labelId="action-select-label" {...props}>
        {req.data.map((el: IDisplayValue) => (
          <MenuItem key={`${el.Id}-${el.Value}`} value={el.Id}>
            {el.Label}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}
