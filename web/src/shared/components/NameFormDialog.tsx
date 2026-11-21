import { TextField } from "@mui/material";
import { useState } from "react";
import useFormErrors from "../hooks/useFormErrors";
import FormDialog from "./FormDialog";

interface NameFormDialogProps {
  title: string;
  /** Nom actuel en renommage ; absent en création. */
  initialName?: string;
  submitting: boolean;
  /** Erreur de l'API (ex. nom en double) : en alerte, ou sous le champ si `onEdit` est fourni. */
  error: string | null;
  onSubmit: (name: string) => void;
  /** Appelé à chaque saisie pour effacer l'erreur de l'API ; elle est alors affichée sous le champ. */
  onEdit?: () => void;
  maxLength?: number;
  onClose: () => void;
}

/** Dialogue de saisie d'un nom (création ou renommage d'une valeur de référentiel). */
export default function NameFormDialog(props: NameFormDialogProps) {
  const { title, initialName, submitting, error, onSubmit, onEdit, maxLength, onClose } = props;
  const [name, setName] = useState(initialName ?? "");
  const { validate, clear, fieldProps } = useFormErrors<"name">();
  const apiErrorUnderField = onEdit !== undefined;

  const handleSubmit = () => {
    if (validate({ name: name.trim() === "" && "Indiquez le nom" })) onSubmit(name.trim());
  };

  const field = fieldProps("name", apiErrorUnderField ? (error ?? undefined) : undefined);
  return (
    <FormDialog
      title={title}
      maxWidth="xs"
      submitLabel="Enregistrer"
      submitting={submitting}
      error={apiErrorUnderField ? null : error}
      onSubmit={handleSubmit}
      onClose={onClose}
    >
      <TextField
        label="Nom"
        required
        fullWidth
        margin="dense"
        value={name}
        {...field}
        error={field.error || (apiErrorUnderField && error !== null)}
        slotProps={{ htmlInput: { maxLength } }}
        onChange={(e) => {
          setName(e.target.value);
          clear("name");
          onEdit?.();
        }}
      />
    </FormDialog>
  );
}
