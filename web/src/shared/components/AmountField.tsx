import { InputAdornment, TextField } from "@mui/material";
import { parseAmount } from "../format/parseAmount";

interface AmountFieldProps {
  value: string;
  onChange: (value: string) => void;
  label?: string;
  /** Remplace « Facultatif » (ex. champ obligatoire). */
  hint?: string;
}

/** Champ « Montant (€) » facultatif : clavier décimal sur mobile, virgule française acceptée. */
export default function AmountField({ value, onChange, label = "Montant (€)", hint = "Facultatif" }: AmountFieldProps) {
  const { error } = parseAmount(value);
  return (
    <TextField
      label={label}
      fullWidth
      margin="dense"
      value={value}
      error={error !== null}
      helperText={error ?? hint}
      slotProps={{
        htmlInput: { inputMode: "decimal", maxLength: 16 },
        input: { endAdornment: <InputAdornment position="end">€</InputAdornment> },
      }}
      onChange={(e) => onChange(e.target.value)}
    />
  );
}
