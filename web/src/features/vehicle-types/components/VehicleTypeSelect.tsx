import { MenuItem, TextField } from "@mui/material";
import { useVehicleTypes } from "../api/vehicleTypeQueries";

interface VehicleTypeSelectProps {
  label?: string;
  /** Id du type sélectionné, "" pour aucun. */
  value: string;
  onChange: (typeId: string) => void;
  /** Libellé de « aucun » (ex. « Tous » pour un filtre). */
  emptyLabel: string;
  /** Type actuel du véhicule, conservé affiché même désactivé. */
  current?: { id: string; name: string } | null;
  /** Inclut les types désactivés (filtre de la liste). */
  includeInactive?: boolean;
  size?: "small" | "medium";
  minWidth?: number;
}

/** Sélecteur de type : types actifs uniquement (sauf `includeInactive`), plus le type actuel s'il est désactivé. */
export default function VehicleTypeSelect(props: VehicleTypeSelectProps) {
  const { label = "Type", value, onChange, emptyLabel, current, includeInactive = false, size, minWidth } = props;
  const types = useVehicleTypes(includeInactive);

  const options = (types.data ?? []).map((t) => ({ id: t.id, name: t.isActive ? t.name : `${t.name} (désactivé)` }));
  const orphan = current && !options.some((o) => o.id === current.id) ? current : null;
  // Tant que la liste charge, la valeur sélectionnée doit rester valide pour MUI.
  const known = value === "" || options.some((o) => o.id === value) || orphan?.id === value;

  return (
    <TextField
      select
      label={label}
      size={size}
      fullWidth={minWidth === undefined}
      value={known ? value : ""}
      onChange={(e) => onChange(e.target.value)}
      sx={minWidth ? { minWidth } : undefined}
    >
      <MenuItem value="">{emptyLabel}</MenuItem>
      {orphan && <MenuItem value={orphan.id}>{`${orphan.name} (désactivé)`}</MenuItem>}
      {options.map((o) => (
        <MenuItem key={o.id} value={o.id}>
          {o.name}
        </MenuItem>
      ))}
    </TextField>
  );
}
