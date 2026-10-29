import { Button, Paper, Stack, Table, TableBody, TableCell, TableHead, TableRow, Typography } from "@mui/material";
import { useState } from "react";
import useNotifSnack from "../../../hooks/useNotifSnack";
import { useInterventions } from "../../maintenance/api/interventionQueries";
import { formatDateOnly } from "../../vehicles/components/vehicleForm";
import { problemErrorMessage } from "../api/problemErrors";
import { useAddProblemAction, useCloseProblemAction, useUpdateProblemAction } from "../api/problemMutations";
import { useReferenceNames } from "../hooks/useReferenceNames";
import type { Problem, ProblemAction } from "../types/problem";
import CloseActionDialog from "./CloseActionDialog";
import ProblemActionDialog from "./ProblemActionDialog";

type Dialog = { kind: "add" } | { kind: "edit"; action: ProblemAction } | { kind: "close"; action: ProblemAction };

/** Actions de résolution d'un problème analysé : ajout, modification et clôture (avec confirmation). */
export default function ProblemActions({ problem }: { problem: Problem }) {
  const { notifySuccess } = useNotifSnack();
  const name = useReferenceNames();
  const interventions = useInterventions(problem.vehicleId);
  const add = useAddProblemAction(problem.id, problem.vehicleId);
  const update = useUpdateProblemAction(problem.id, problem.vehicleId);
  const close = useCloseProblemAction(problem.id, problem.vehicleId);
  const [dialog, setDialog] = useState<Dialog | null>(null);

  const dismiss = () => {
    setDialog(null);
    add.reset();
    update.reset();
    close.reset();
  };
  const done = (message: string) => () => {
    notifySuccess(message);
    dismiss();
  };
  const interventionLabel = (id: string | null) => {
    if (!id) return "-";
    const found = interventions.data?.find((i) => i.id === id);
    return found ? `${formatDateOnly(found.date)} - ${found.work}` : "-";
  };

  return (
    <Paper sx={{ padding: 2 }}>
      <Stack direction="row" justifyContent="space-between" alignItems="center" marginBottom={1}>
        <Typography variant="h6">Actions</Typography>
        {problem.status !== "Reported" && (
          <Button variant="outlined" onClick={() => setDialog({ kind: "add" })}>
            Ajouter une action
          </Button>
        )}
      </Stack>
      {problem.actions.length === 0 ? (
        <Typography color="text.secondary">Aucune action.</Typography>
      ) : (
        <Table size="small" aria-label="Actions du problème">
          <TableHead>
            <TableRow>
              {["Acteur", "Type", "Contrainte", "Échéance", "Commentaire", "Intervention", "État", ""].map((h) => (
                <TableCell key={h}>{h}</TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {problem.actions.map((a) => (
              <TableRow key={a.id}>
                <TableCell>{name("actor", a.actorId)}</TableCell>
                <TableCell>{name("actionType", a.typeId)}</TableCell>
                <TableCell>{name("constraint", a.constraintId)}</TableCell>
                <TableCell>{formatDateOnly(a.dueDate)}</TableCell>
                <TableCell>{a.comment ?? "-"}</TableCell>
                <TableCell>{interventionLabel(a.interventionId)}</TableCell>
                <TableCell>{a.isClosed ? `Clôturée le ${formatDateOnly(a.closedAt?.slice(0, 10) ?? null)}` : "Ouverte"}</TableCell>
                <TableCell align="right">
                  {!a.isClosed && (
                    <>
                      <Button size="small" onClick={() => setDialog({ kind: "edit", action: a })}>
                        Modifier
                      </Button>
                      <Button size="small" onClick={() => setDialog({ kind: "close", action: a })}>
                        Clôturer
                      </Button>
                    </>
                  )}
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      )}
      {dialog?.kind === "add" && (
        <ProblemActionDialog
          vehicleId={problem.vehicleId}
          submitting={add.isPending}
          error={add.isError ? problemErrorMessage(add.error) : null}
          onClose={dismiss}
          onSubmit={(v) => add.mutate(v, { onSuccess: done("Action ajoutée") })}
        />
      )}
      {dialog?.kind === "edit" && (
        <ProblemActionDialog
          vehicleId={problem.vehicleId}
          action={dialog.action}
          submitting={update.isPending}
          error={update.isError ? problemErrorMessage(update.error) : null}
          onClose={dismiss}
          onSubmit={({ dueDate, comment, interventionId }) =>
            update.mutate(
              { actionId: dialog.action.id, input: { dueDate, comment, interventionId } },
              { onSuccess: done("Action modifiée") },
            )
          }
        />
      )}
      {dialog?.kind === "close" && (
        <CloseActionDialog
          submitting={close.isPending}
          error={close.isError ? problemErrorMessage(close.error) : null}
          onClose={dismiss}
          onConfirm={() => close.mutate(dialog.action.id, { onSuccess: done("Action clôturée") })}
        />
      )}
    </Paper>
  );
}
