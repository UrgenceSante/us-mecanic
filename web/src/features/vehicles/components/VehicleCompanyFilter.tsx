import { MenuItem, TextField } from "@mui/material";
import { useCompanies } from "../../companies/api/companyQueries";

/** Valeur du filtre « Propriétaire » pour les véhicules sans propriétaire. */
export const OWNER_MISSING = "none";

interface VehicleCompanyFilterProps {
  label: string;
  /** "" = tous, OWNER_MISSING (si `allowMissing`) ou id de société. */
  value: string;
  onChange: (value: string) => void;
  /** Propose « À renseigner » (filtre propriétaire uniquement). */
  allowMissing?: boolean;
}

/** Filtre du parc par société : toutes les sociétés (même désactivées, elles peuvent encore posséder des véhicules). */
export default function VehicleCompanyFilter({ label, value, onChange, allowMissing = false }: VehicleCompanyFilterProps) {
  const companies = useCompanies(true).data ?? [];
  const known = value === "" || value === OWNER_MISSING || companies.some((c) => c.id === value);
  return (
    <TextField
      select
      label={label}
      size="small"
      value={known ? value : ""}
      onChange={(e) => onChange(e.target.value)}
      sx={{ minWidth: 200 }}
    >
      <MenuItem value="">Tous</MenuItem>
      {allowMissing && <MenuItem value={OWNER_MISSING}>À renseigner</MenuItem>}
      {companies.map((c) => (
        <MenuItem key={c.id} value={c.id}>
          {c.isActive ? c.name : `${c.name} (désactivée)`}
        </MenuItem>
      ))}
    </TextField>
  );
}
