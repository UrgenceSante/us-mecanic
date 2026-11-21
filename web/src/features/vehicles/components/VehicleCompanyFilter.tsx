import { MenuItem, TextField, Tooltip } from "@mui/material";
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
  /** Explication de la règle, en info-bulle et description accessible. */
  hint?: string;
  /** Largeur minimale alignée sur les autres filtres de la page (sinon : mise en page du parc). */
  minWidth?: number;
}

/** Filtre du parc par société : toutes les sociétés (même désactivées, elles peuvent encore posséder des véhicules). */
export default function VehicleCompanyFilter({ label, value, onChange, allowMissing = false, hint, minWidth }: VehicleCompanyFilterProps) {
  const companies = useCompanies(true).data ?? [];
  const known = value === "" || value === OWNER_MISSING || companies.some((c) => c.id === value);
  const field = (
    <TextField
      select
      label={label}
      size="small"
      value={known ? value : ""}
      onChange={(e) => onChange(e.target.value)}
      sx={minWidth ? { minWidth } : { minWidth: 170, flex: { xs: "1 1 40%", md: "0 0 auto" } }}
      slotProps={hint ? { select: { SelectDisplayProps: { "aria-description": hint } } } : undefined}
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
  return hint ? <Tooltip title={hint}>{field}</Tooltip> : field;
}
