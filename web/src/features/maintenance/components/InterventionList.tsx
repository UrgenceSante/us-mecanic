import DeleteIcon from "@mui/icons-material/Delete";
import { Chip, Divider, IconButton, Stack, Typography } from "@mui/material";
import { Fragment } from "react";
import { formatKm } from "../../mileage/types/format";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { Intervention } from "../types/intervention";

interface InterventionListProps {
  interventions: Intervention[];
  /** Absent pour un véhicule sorti du parc : l'action « Supprimer » est alors masquée. */
  onDelete?: (intervention: Intervention) => void;
}

/** Carnet d'entretien : interventions du plus récent au plus ancien (ordre fourni par l'API). */
export default function InterventionList({ interventions, onDelete }: InterventionListProps) {
  return (
    <Stack gap={1} component="ul" sx={{ listStyle: "none", margin: 0, padding: 0 }}>
      {interventions.map((i, index) => (
        <Fragment key={i.id}>
          {index > 0 && <Divider component="li" />}
          <Stack component="li" gap={0.5}>
            <Stack direction="row" alignItems="center">
              <Typography fontWeight="bold" flexGrow={1}>
                {formatDateOnly(i.date)}
                {i.kilometers != null && ` · ${formatKm(i.kilometers)}`}
                {i.garage && ` · ${i.garage.name}`}
              </Typography>
              {onDelete && (
                <IconButton aria-label={`Supprimer l'intervention du ${formatDateOnly(i.date)}`} onClick={() => onDelete(i)}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              )}
            </Stack>
            <Typography sx={{ whiteSpace: "pre-wrap" }}>{i.work}</Typography>
            {i.deadlines.length > 0 && (
              <Stack direction="row" gap={0.5} flexWrap="wrap">
                {i.deadlines.map((d) => (
                  <Chip key={d.id} size="small" label={d.typeName ?? "Échéance"} />
                ))}
              </Stack>
            )}
          </Stack>
        </Fragment>
      ))}
    </Stack>
  );
}
