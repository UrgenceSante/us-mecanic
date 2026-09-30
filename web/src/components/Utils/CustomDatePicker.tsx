import { type TextFieldProps } from "@mui/material";
import { DatePicker } from "@mui/x-date-pickers/DatePicker";
import { AdapterDateFns } from "@mui/x-date-pickers/AdapterDateFns";
import { LocalizationProvider } from "@mui/x-date-pickers/LocalizationProvider";
import { fr } from "date-fns/locale/fr"; // pour afficher en français

interface CustomDatePickerProps
  extends Omit<TextFieldProps, "value" | "onChange"> {
  value: Date | null;
  onChange: (newDate: Date | null) => void;
  label?: string;
  disabled?: boolean;
}

export default function CustomDatePicker({
  value,
  onChange,
  label,
  disabled,
  ...textFieldProps
}: CustomDatePickerProps) {
  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={fr}>
      <DatePicker
        value={value}
        onChange={onChange}
        label={label}
        disabled={disabled}
        slotProps={{
          textField: {
            size: "small",
            ...textFieldProps,
          },
        }}
      />
    </LocalizationProvider>
  );
}
