import {
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  Skeleton,
  type SelectProps,
} from "@mui/material";
import ErrorHandler from "../../../Utils/Error/ErrorHandler";
import type { UseQueryResult } from "@tanstack/react-query";
import type { IDisplayValue } from "../../IAnalyse";

interface AsyncSelectProps {
  req: UseQueryResult<IDisplayValue[], Error>;
  selectProps?: SelectProps<string>;
}
export default function AsyncSelect(props: AsyncSelectProps) {
  const { req } = props;

  if (req.isLoading) return <Skeleton width={150} height={70} />;
  if (req.isError) return <ErrorHandler error={req.error} />;

  return (
    <FormControl sx={{ minWidth: 150, flex: 1 }} size="small">
      <InputLabel id="action-select-label">
        {props.selectProps?.label}
      </InputLabel>
      <Select
        {...props.selectProps}
        labelId="action-select-label"
        value={props.selectProps?.value}
      >
        {req.data?.map((el) => (
          <MenuItem key={`${el.Id}-${el.Value}`} value={el.Id}>
            {el.Value}
          </MenuItem>
        ))}
      </Select>
    </FormControl>
  );
}
