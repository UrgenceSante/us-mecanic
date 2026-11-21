import { TextField } from "@mui/material";
import { useState } from "react";
import FormDialog from "../../../shared/components/FormDialog";
import FrenchDateField from "../../../shared/components/FrenchDateField";
import useFormErrors from "../../../shared/hooks/useFormErrors";
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

type Field = "actorId" | "typeId" | "constraintId";

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
  const { validate, clear, fieldProps } = useFormErrors<Field>();

  const handleSubmit = () => {
    const valid = validate(
      action
        ? {}
        : {
            actorId: actorId === "" && "Choisissez l'acteur",
            typeId: typeId === "" && "Choisissez le type d'action",
            constraintId: constraintId === "" && "Choisissez la contrainte",
          },
    );
    if (!valid) return;
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
    <FormDialog
      title={action ? "Modifier l'action" : "Ajouter une action"}
      submitLabel="Enregistrer"
      submitting={submitting}
      error={error}
      onSubmit={handleSubmit}
      onClose={props.onClose}
    >
      {!action && (
        <>
          <ReferenceSelect
            kind="actor"
            label="Acteur"
            value={actorId}
            onChange={(v) => {
              setActorId(v);
              clear("actorId");
            }}
            {...fieldProps("actorId")}
          />
          <ReferenceSelect
            kind="actionType"
            label="Type d'action"
            value={typeId}
            onChange={(v) => {
              setTypeId(v);
              clear("typeId");
            }}
            {...fieldProps("typeId")}
          />
          <ReferenceSelect
            kind="constraint"
            label="Contrainte"
            value={constraintId}
            onChange={(v) => {
              setConstraintId(v);
              clear("constraintId");
            }}
            {...fieldProps("constraintId")}
          />
        </>
      )}
      <FrenchDateField layout="form" label="Échéance" value={dueDate} onChange={setDueDate} />
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
    </FormDialog>
  );
}
