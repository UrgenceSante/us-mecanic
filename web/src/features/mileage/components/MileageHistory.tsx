import DeleteIcon from "@mui/icons-material/Delete";
import EditIcon from "@mui/icons-material/Edit";
import { IconButton, List, ListItem, ListItemText } from "@mui/material";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { formatKm } from "../types/format";
import type { MileageReading } from "../types/mileageReading";

interface MileageHistoryProps {
  readings: MileageReading[];
  /** Faux pour un véhicule sorti du parc : plus de correction possible. */
  editable: boolean;
  onEdit: (reading: MileageReading) => void;
  onDelete: (reading: MileageReading) => void;
}

export default function MileageHistory({ readings, editable, onEdit, onDelete }: MileageHistoryProps) {
  return (
    <List dense aria-label="Historique des relevés">
      {readings.map((r) => (
        <ListItem
          key={r.id}
          disableGutters
          secondaryAction={
            editable ? (
              <>
                <IconButton aria-label={`Modifier le relevé du ${formatDateOnly(r.date)}`} onClick={() => onEdit(r)}>
                  <EditIcon fontSize="small" />
                </IconButton>
                <IconButton aria-label={`Supprimer le relevé du ${formatDateOnly(r.date)}`} onClick={() => onDelete(r)}>
                  <DeleteIcon fontSize="small" />
                </IconButton>
              </>
            ) : undefined
          }
        >
          <ListItemText
            primary={`${formatDateOnly(r.date)} : ${formatKm(r.kilometers)}`}
            secondary={r.reason ? `Motif : ${r.reason}` : undefined}
          />
        </ListItem>
      ))}
    </List>
  );
}
