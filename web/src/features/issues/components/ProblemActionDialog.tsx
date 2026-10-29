import { Alert, Button, Dialog, DialogActions, DialogContent, DialogTitle, TextField } from "@mui/material";
import { useState, type FormEvent } from "react";
import type { ProblemAction, UpdateProblemActionInput } from "../types/problem";
import InterventionSelect from "./InterventionSelect";
import ReferenceSelect from "./ReferenceSelect";

interface ProblemActionDialogProps {
  vehicleId: string;
  /** Action à modifier ; absente pour un ajout. */
  action?: ProblemAction;
  submitting: boolean;
  error: string | null;
  onSubmit: (values: {
    actorId: string;
    typeId: string;
    constraintId: string;
  } & UpdateProblemActionInput) => void;
  onClose: () => void;
}

function initialValues(action?: ProblemAction) {
  return {
    actorId: action?.actorId ?? "",
    typeId: action?.typeId ?? "",
    constraintId: action?.constraintId ?? "",
    dueDate: action?.dueDate ?? "",
    comment: action?.comment ?? "",
    interventionId: action?.interventionId ?? "",
  };
}

/** Ajout (acteur, type, contrainte figés ensuite) ou modification (échéance, commentaire, intervention) d'une action. */
export default function ProblemActionDialog(props: ProblemActionDialogProps) {
  const { vehicleId, action, submitting, error } = props;
  const initial = initialValues(action);
  const [actorId, setActorId] = useState(initial.actorId);
  const [typeId, setTypeId] = useState(initial.typeId);
  const [constraintId, setConstraintId] = useState(initial.constraintId);
  const [dueDate, setDueDate] = useState(initial.dueDate);
  const [comment, setComment] = useState(initial.comment);
  const [interventionId, setInterventionId] = useState(initial.interventionId);

  const invalid = !action && (actorId === "" || typeId === "" || constraintId === "");

  const handleSubmit = (e: FormEvent) => {
    e.preventDefault();
    if (invalid) return;
    props.onSubmit({
      actorId,
      typeId,
      constraintId,
      dueDate: dueDate === "" ? null : dueDate,
      comment: comment.trim() === "" ? null : comment.trim(),
      interventionId: interventionId === "" ? null : interventionId,
    });
  };

  return (
    <Dialog open onClose={props.onClose} fullWidth maxWidth="sm">
      <form onSubmit={handleSubmit}>
        <DialogTitle>{action ? "Modifier l'action" : "Ajouter une action"}</DialogTitle>
        <DialogContent>
          {error && (
            <Alert severity="error" sx={{ marginBottom: 1 }}>
              {error}
            </Alert>
          )}
          {!action && (
            <>
              <ReferenceSelect kind="actor" label="Acteur" value={actorId} onChange={setActorId} />
              <ReferenceSelect kind="actionType" label="Type d'action" value={typeId} onChange={setTypeId} />
              <ReferenceSelect kind="constraint" label="Contrainte" value={constraintId} onChange={setConstraintId} />
            </>
          )}
          <TextField
            label="Échéance"
            type="date"
            fullWidth
            margin="dense"
            value={dueDate}
            slotProps={{ inputLabel: { shrink: true } }}
            onChange={(e) => setDueDate(e.target.value)}
          />
          <TextField
            label="Commentaire"
            fullWidth
            multiline
            minRows={2}
            margin="dense"
            value={comment}
            onChange={(e) => setComment(e.target.value)}
          />
          <InterventionSelect vehicleId={vehicleId} value={interventionId} onChange={setInterventionId} />
        </DialogContent>
        <DialogActions>
          <Button onClick={props.onClose}>Annuler</Button>
          <Button type="submit" variant="contained" disabled={invalid || submitting}>
            Enregistrer
          </Button>
        </DialogActions>
      </form>
    </Dialog>
  );
}
