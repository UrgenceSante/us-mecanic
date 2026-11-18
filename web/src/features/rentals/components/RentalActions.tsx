import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import EventBusyIcon from "@mui/icons-material/EventBusy";
import { IconButton, Stack, Tooltip } from "@mui/material";
import type { Rental } from "../types/rental";

export interface RentalActionHandlers {
  onEnd: (rental: Rental) => void;
  onEdit: (rental: Rental) => void;
  onDelete: (rental: Rental) => void;
}

const touchTarget = { width: 44, height: 44 };

/** Terminer (location en cours sans date de fin), Modifier, Supprimer. */
export default function RentalActions({ rental, onEnd, onEdit, onDelete }: RentalActionHandlers & { rental: Rental }) {
  const name = `à ${rental.tenant.name}`;
  return (
    <Stack direction="row" justifyContent="flex-end" flexWrap="nowrap">
      {rental.state === "Ongoing" && rental.endDate === null && (
        <Tooltip title="Terminer">
          <IconButton sx={touchTarget} aria-label={`Terminer la location ${name}`} onClick={() => onEnd(rental)}>
            <EventBusyIcon />
          </IconButton>
        </Tooltip>
      )}
      <Tooltip title="Modifier">
        <IconButton sx={touchTarget} aria-label={`Modifier la location ${name}`} onClick={() => onEdit(rental)}>
          <EditIcon />
        </IconButton>
      </Tooltip>
      <Tooltip title="Supprimer">
        <IconButton sx={touchTarget} aria-label={`Supprimer la location ${name}`} onClick={() => onDelete(rental)}>
          <DeleteIcon />
        </IconButton>
      </Tooltip>
    </Stack>
  );
}
