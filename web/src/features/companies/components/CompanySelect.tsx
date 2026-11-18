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
}

/** Sélecteur de société : sociétés actives uniquement, plus la société actuelle si elle est inactive. */
export default function CompanySelect(props: CompanySelectProps) {
  const { label, value, onChange, emptyLabel, allowEmpty = true, current, required, error, helperText, autoFocus } = props;
  const companies = useCompanies(false);

  const options = companies.data ?? [];
  const orphan = current && !current.isActive ? current : null;
  // Tant que la liste charge, la valeur sélectionnée doit rester valide pour MUI.
  const known = value === "" || options.some((c) => c.id === value) || orphan?.id === value;

  return (
    <TextField
      select
      fullWidth
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
            return options.find((c) => c.id === id)?.name ?? "";
          },
        },
      }}
    >
      {allowEmpty && <MenuItem value="">{emptyLabel}</MenuItem>}
      {orphan && <MenuItem value={orphan.id}>{`${orphan.name} (désactivée)`}</MenuItem>}
      {options.map((c) => (
        <MenuItem key={c.id} value={c.id}>
          {c.name}
        </MenuItem>
      ))}
    </TextField>
  );
}
