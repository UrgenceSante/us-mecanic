import useNotifSnack from "../../../hooks/useNotifSnack";
import { useAddRental, useDeleteRental, useEndRental, useUpdateRental } from "../api/rentalQueries";
import type { Rental, RentalInput } from "../types/rental";
import DeleteRentalDialog from "./DeleteRentalDialog";
import EndRentalDialog from "./EndRentalDialog";
import RentalFormDialog from "./RentalFormDialog";

/** Boîte de dialogue ouverte : ajout, ou action sur une location existante. */
export type RentalDialog = { kind: "add" } | { kind: "edit" | "end" | "delete"; rental: Rental };

interface RentalDialogsProps {
  vehicleId: string;
  ownerId?: string;
  dialog: RentalDialog | null;
  onClose: () => void;
}

/** Dialogues de la section : chacun porte sa mutation, et son erreur s'affiche à côté du champ concerné. */
export default function RentalDialogs({ vehicleId, ownerId, dialog, onClose }: RentalDialogsProps) {
  const { notifySuccess } = useNotifSnack();
  const add = useAddRental(vehicleId);
  const update = useUpdateRental(vehicleId);
  const end = useEndRental(vehicleId);
  const remove = useDeleteRental(vehicleId);

  if (!dialog) return null;
  const done = (message: string) => ({
    onSuccess: () => {
      notifySuccess(message);
      onClose();
    },
  });

  if (dialog.kind === "end") {
    return (
      <EndRentalDialog
        rental={dialog.rental}
        submitting={end.isPending}
        error={end.error}
        onConfirm={(endDate) => end.mutate({ rentalId: dialog.rental.id, endDate }, done("Location terminée"))}
        onClose={onClose}
      />
    );
  }
  if (dialog.kind === "delete") {
    return (
      <DeleteRentalDialog
        rental={dialog.rental}
        submitting={remove.isPending}
        error={remove.error}
        onConfirm={() => remove.mutate(dialog.rental.id, done("Location supprimée"))}
        onClose={onClose}
      />
    );
  }
  const editing = dialog.kind === "edit" ? dialog.rental : undefined;
  const mutation = editing ? update : add;
  const submit = (input: RentalInput) =>
    editing
      ? update.mutate({ rentalId: editing.id, input }, done("Location modifiée"))
      : add.mutate(input, done("Location enregistrée"));
  return (
    <RentalFormDialog rental={editing} ownerId={ownerId} submitting={mutation.isPending} error={mutation.error} onSubmit={submit} onClose={onClose} />
  );
}
