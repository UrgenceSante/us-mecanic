import { DatePicker } from "@mui/x-date-pickers";
import { format, isValid, parse } from "date-fns";
import { useMemo } from "react";

interface FrenchDateFieldProps {
  label: string;
  /** Date ISO AAAA-MM-JJ, chaîne vide si non renseignée. */
  value: string;
  onChange: (value: string) => void;
  name?: string;
  required?: boolean;
  disabled?: boolean;
  /** Erreur affichée à côté du champ. */
  error?: boolean;
  helperText?: string;
  /** Date maximale ISO AAAA-MM-JJ (ex. aujourd'hui). */
  maxDate?: string;
  /** `form` : pleine largeur et taille standard, comme les autres champs d'un dialogue ; `inline` (défaut) : compact. */
  layout?: "inline" | "form";
}

const isoFormat = "yyyy-MM-dd";

const toDate = (iso: string): Date | null => {
  if (!iso) return null;
  const d = parse(iso, isoFormat, new Date());
  return isValid(d) ? d : null;
};

/**
 * Sélecteur de date affiché JJ/MM/AAAA quelle que soit la langue du navigateur ; l'état reste en ISO.
 * Nécessite `FrenchLocalizationProvider` en amont (placé à la racine dans `main.tsx`).
 */
export default function FrenchDateField({
  label,
  value,
  onChange,
  name,
  required,
  disabled,
  error,
  helperText,
  maxDate,
  layout = "inline",
}: FrenchDateFieldProps) {
  const date = useMemo(() => toDate(value), [value]);
  const max = useMemo(() => (maxDate ? (toDate(maxDate) ?? undefined) : undefined), [maxDate]);
  return (
    <DatePicker
      label={label}
      value={date}
      maxDate={max}
      disabled={disabled}
      onChange={(d) => onChange(d && isValid(d) ? format(d, isoFormat) : "")}
      slotProps={{
        textField:
          layout === "form"
            ? { name, fullWidth: true, margin: "dense", required, error, helperText }
            : { name, size: "small", required, error, helperText, sx: { flex: 1, minWidth: 0 } },
      }}
    />
  );
}
