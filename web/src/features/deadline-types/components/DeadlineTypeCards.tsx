import BlockIcon from "@mui/icons-material/Block";
import ActiveStatusChip from "../../shared/components/ActiveStatusChip";
import EditIcon from "@mui/icons-material/Edit";
import PlaylistAddIcon from "@mui/icons-material/PlaylistAdd";
import { IconButton, Stack, Typography } from "@mui/material";
import ListCard from "../../../shared/components/ListCard";
import type { DeadlineType } from "../types/deadlineType";
import { formatPeriodicity, formatThresholds } from "../types/format";

interface Props {
  types: DeadlineType[];
  deactivating: boolean;
  onEdit: (type: DeadlineType) => void;
  onAssign: (type: DeadlineType) => void;
  onDeactivate: (type: DeadlineType) => void;
}

const touchTarget = { width: 44, height: 44 };

/** Rendu mobile : un type d'échéance par carte. Les seuils globaux ne sont pas répétés. */
export default function DeadlineTypeCards({ types, deactivating, onEdit, onAssign, onDeactivate }: Props) {
  return (
    <Stack gap={1} padding={1}>
      <Stack gap={1} role="list" aria-label="Types d'échéance">
        {types.map((type) => {
          const thresholds = formatThresholds(type);
          const summary = [formatPeriodicity(type), thresholds === "seuils globaux" ? null : `Seuils : ${thresholds}`];
          return (
            <ListCard key={type.id}>
              <Stack gap={0.5}>
                <Stack direction="row" alignItems="center" justifyContent="space-between" gap={1}>
                  <Typography fontWeight="bold">{type.name}</Typography>
                  <ActiveStatusChip isActive={type.isActive} />
                </Stack>
                <Typography variant="body2" color="text.secondary">
                  {summary.filter(Boolean).join(" · ")}
                </Typography>
                <Stack direction="row" justifyContent="flex-end" flexWrap="nowrap">
                  <IconButton sx={touchTarget} aria-label={`Modifier ${type.name}`} onClick={() => onEdit(type)}>
                    <EditIcon />
                  </IconButton>
                  {type.isActive && !type.isOneOff && (
                    <IconButton
                      sx={touchTarget}
                      aria-label={`Ajouter ${type.name} à des véhicules`}
                      onClick={() => onAssign(type)}
                    >
                      <PlaylistAddIcon />
                    </IconButton>
                  )}
                  {type.isActive && (
                    <IconButton
                      sx={touchTarget}
                      aria-label={`Désactiver ${type.name}`}
                      disabled={deactivating}
                      onClick={() => onDeactivate(type)}
                    >
                      <BlockIcon />
                    </IconButton>
                  )}
                </Stack>
              </Stack>
            </ListCard>
          );
        })}
      </Stack>
      {types.length === 0 && <Typography>Aucun type d'échéance.</Typography>}
    </Stack>
  );
}
