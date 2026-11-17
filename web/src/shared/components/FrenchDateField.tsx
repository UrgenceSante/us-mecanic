import { DatePicker, LocalizationProvider } from "@mui/x-date-pickers";
import { AdapterDateFns } from "@mui/x-date-pickers/AdapterDateFns";
import { format, isValid, parse } from "date-fns";
import { useMemo } from "react";
import { fr } from "date-fns/locale";

interface FrenchDateFieldProps {
  label: string;
  /** Date ISO AAAA-MM-JJ, chaîne vide si non renseignée. */
  value: string;
  onChange: (value: string) => void;
}

const isoFormat = "yyyy-MM-dd";

const toDate = (iso: string): Date | null => {
  if (!iso) return null;
  const d = parse(iso, isoFormat, new Date());
  return isValid(d) ? d : null;
};

/** Sélecteur de date affiché JJ/MM/AAAA quelle que soit la langue du navigateur ; l'état reste en ISO. */
export default function FrenchDateField({ label, value, onChange }: FrenchDateFieldProps) {
  const date = useMemo(() => toDate(value), [value]);
  return (
    <LocalizationProvider dateAdapter={AdapterDateFns} adapterLocale={fr}>
      <DatePicker
        label={label}
        value={date}
        onChange={(d) => onChange(d && isValid(d) ? format(d, isoFormat) : "")}
        slotProps={{ textField: { size: "small", sx: { flex: 1, minWidth: 0 } } }}
      />
    </LocalizationProvider>
  );
}
