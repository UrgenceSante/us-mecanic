import { Button, Stack, Typography } from "@mui/material";
import ListCard from "../../../shared/components/ListCard";
import { formatParisDate } from "../../../shared/format/formatParisDate";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import type { ProblemReferenceKind } from "../../problem-references/types/problemReference";
import type { ProblemAction } from "../types/problem";

interface Props {
  actions: ProblemAction[];
  name: (kind: ProblemReferenceKind, id: string) => string;
  interventionLabel: (id: string | null) => string;
  onEdit: (action: ProblemAction) => void;
  onClose: (action: ProblemAction) => void;
}

/** Rendu mobile / tablette : une action par carte. */
export default function ProblemActionCards({ actions, name, interventionLabel, onEdit, onClose }: Props) {
  return (
    <Stack gap={1} aria-label="Actions du problème">
      {actions.map((a) => (
        <ListCard key={a.id}>
          <Stack gap={0.5}>
            <Typography fontWeight="bold">
              {name("actor", a.actorId)} · {name("actionType", a.typeId)}
            </Typography>
            <Typography variant="body2">
              Échéance : {formatDateOnly(a.dueDate)} · Contrainte : {name("constraint", a.constraintId)}
            </Typography>
            {a.comment && (
              <Typography variant="body2" sx={{ overflowWrap: "anywhere" }}>
                {a.comment}
              </Typography>
            )}
            {a.interventionId && (
              <Typography variant="body2" color="text.secondary">
                Intervention : {interventionLabel(a.interventionId)}
              </Typography>
            )}
            <Typography variant="body2" color="text.secondary">
              {a.isClosed ? `Clôturée le ${a.closedAt ? formatParisDate(a.closedAt) : "-"}` : "Ouverte"}
            </Typography>
            {!a.isClosed && (
              <Stack direction="row" gap={1} justifyContent="flex-end">
                <Button size="small" onClick={() => onEdit(a)}>
                  Modifier
                </Button>
                <Button size="small" onClick={() => onClose(a)}>
                  Clôturer
                </Button>
              </Stack>
            )}
          </Stack>
        </ListCard>
      ))}
    </Stack>
  );
}
