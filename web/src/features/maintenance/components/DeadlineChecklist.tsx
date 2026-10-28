import { Checkbox, FormControlLabel, FormGroup, Typography } from "@mui/material";
import type { VehicleDeadline } from "../../vehicle-deadlines/types/vehicleDeadline";

interface DeadlineChecklistProps {
  deadlines: VehicleDeadline[];
  checked: string[];
  onToggle: (id: string) => void;
}

/** Cases à cocher des échéances en cours réalisées par l'intervention. */
export default function DeadlineChecklist({ deadlines, checked, onToggle }: DeadlineChecklistProps) {
  if (deadlines.length === 0) return null;
  return (
    <>
      <Typography variant="subtitle2" marginTop={1}>
        Échéances réalisées
      </Typography>
      <FormGroup>
        {deadlines.map((d) => (
          <FormControlLabel
            key={d.id}
            label={d.typeName ?? "Échéance"}
            control={<Checkbox checked={checked.includes(d.id)} onChange={() => onToggle(d.id)} />}
          />
        ))}
      </FormGroup>
    </>
  );
}
