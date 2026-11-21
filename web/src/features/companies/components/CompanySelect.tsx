import { MenuItem, TextField } from "@mui/material";
import type { Company } from "../types/company";
import { useCompanies } from "../api/companyQueries";

interface CompanySelectProps {
  label: string;
  /** Id de la société sélectionnée, "" pour aucune. */
  value: string;
  onChange: (companyId: string) => void;
  /** Libellé de « aucune » (ex. « À renseigner »). */
  emptyLabel: string;
  /** Propose l'entrée « aucune » (faux quand la valeur ne peut plus être vidée). */
  allowEmpty?: boolean;
  /** Société actuelle, conservée affichée même inactive. */
  current?: Company | null;
  required?: boolean;
  error?: boolean;
  helperText?: string;
  autoFocus?: boolean;
  /** Propose aussi les sociétés désactivées, suffixées « (désactivée) » (filtres de consultation). */
  includeInactive?: boolean;
  size?: "small" | "medium";
}

/** Sélecteur de société : sociétés actives uniquement, plus la société actuelle si elle est inactive. */
export default function CompanySelect(props: CompanySelectProps) {
  const { label, value, onChange, emptyLabel, allowEmpty = true, current, required, error, helperText, autoFocus, includeInactive = false, size } = props;
  const companies = useCompanies(includeInactive);

  const options = companies.data ?? [];
  const orphan = current && !current.isActive ? current : null;
  // Tant que la liste charge, la valeur sélectionnée doit rester valide pour MUI.
  const known = value === "" || options.some((c) => c.id === value) || orphan?.id === value;

  return (
    <TextField
      select
      fullWidth
      size={size}
      label={label}
      required={required}
      error={error}
      helperText={helperText}
      autoFocus={autoFocus}
      value={known ? value : ""}
      onChange={(e) => onChange(e.target.value)}
      slotProps={{
        inputLabel: { shrink: true },
        select: {
          displayEmpty: true,
          renderValue: (selected) => {
            const id = selected as string;
            if (id === "") return emptyLabel;
            if (orphan?.id === id) return `${orphan.name} (désactivée)`;
            const found = options.find((c) => c.id === id);
            return found ? (found.isActive ? found.name : `${found.name} (désactivée)`) : "";
          },
        },
      }}
    >
      {allowEmpty && <MenuItem value="">{emptyLabel}</MenuItem>}
      {orphan && <MenuItem value={orphan.id}>{`${orphan.name} (désactivée)`}</MenuItem>}
      {options.map((c) => (
        <MenuItem key={c.id} value={c.id}>
          {c.isActive ? c.name : `${c.name} (désactivée)`}
        </MenuItem>
      ))}
    </TextField>
  );
}
