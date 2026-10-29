import { MenuItem, TextField } from "@mui/material";
import { useInterventions } from "../../maintenance/api/interventionQueries";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";

interface InterventionSelectProps {
  vehicleId: string;
  /** Id de l'intervention liée, chaîne vide pour aucune. */
  value: string;
  onChange: (id: string) => void;
}

/** Choix facultatif d'une intervention, limité à celles du véhicule du problème. */
export default function InterventionSelect({ vehicleId, value, onChange }: InterventionSelectProps) {
  const interventions = useInterventions(vehicleId);
  const list = interventions.data ?? [];
  return (
    <TextField
      select
      fullWidth
      margin="dense"
      label="Intervention liée"
      value={list.some((i) => i.id === value) ? value : ""}
      onChange={(e) => onChange(e.target.value)}
    >
      <MenuItem value="">Aucune</MenuItem>
      {list.map((i) => (
        <MenuItem key={i.id} value={i.id}>
          {formatDateOnly(i.date)} - {i.work}
        </MenuItem>
      ))}
    </TextField>
  );
}
