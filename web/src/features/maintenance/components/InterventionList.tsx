import EditIcon from "@mui/icons-material/Edit";
import DeleteIcon from "@mui/icons-material/Delete";
import { Button, Chip, Divider, IconButton, Stack, Tooltip, Typography } from "@mui/material";
import { Fragment } from "react";
import { formatEuro } from "../../../shared/format/formatEuro";
import { formatKm } from "../../mileage/types/format";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { Intervention } from "../types/intervention";

interface InterventionListProps {
  interventions: Intervention[];
  /** Absent pour un véhicule sorti du parc : l'action « Supprimer » est alors masquée. */
  onDelete?: (intervention: Intervention) => void;
  /** Absent pour un véhicule sorti du parc : l'action « Modifier le montant » est alors masquée. */
  onEditAmount?: (intervention: Intervention) => void;
}

const touchTarget = { minWidth: 40, minHeight: 40 };

/** Carnet d'entretien : interventions du plus récent au plus ancien (ordre fourni par l'API). */
export default function InterventionList({ interventions, onDelete, onEditAmount }: InterventionListProps) {
  return (
    <Stack gap={1} component="ul" sx={{ listStyle: "none", margin: 0, padding: 0 }}>
      {interventions.map((i, index) => (
        <Fragment key={i.id}>
          {index > 0 && <Divider component="li" />}
          <Stack component="li" gap={0.5}>
            <Stack direction="row" alignItems="center" gap={0.5}>
              <Typography fontWeight="bold" flexGrow={1}>
                {formatDateOnly(i.date)}
                {i.kilometers != null && ` · ${formatKm(i.kilometers)}`}
                {i.garage && ` · ${i.garage.name}`}
              </Typography>
              {i.amount != null && (
                <Typography fontWeight={500} sx={{ whiteSpace: "nowrap" }}>
                  {formatEuro(i.amount)}
                </Typography>
              )}
              {i.amount == null && onEditAmount && (
                <Button size="small" onClick={() => onEditAmount(i)}>
                  Ajouter un montant
                </Button>
              )}
              {onEditAmount && (
                <Tooltip title="Modifier le montant">
                  <IconButton
                    aria-label={`Modifier le montant de l'intervention du ${formatDateOnly(i.date)}`}
                    sx={touchTarget}
                    onClick={() => onEditAmount(i)}
                  >
                    <EditIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              )}
              {onDelete && (
                <Tooltip title="Supprimer">
                  <IconButton
                    aria-label={`Supprimer l'intervention du ${formatDateOnly(i.date)}`}
                    color="error"
                    sx={touchTarget}
                    onClick={() => onDelete(i)}
                  >
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
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
