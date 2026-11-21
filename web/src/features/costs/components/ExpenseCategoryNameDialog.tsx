import NameFormDialog from "../../../shared/components/NameFormDialog";

interface ExpenseCategoryNameDialogProps {
  /** Nom actuel en renommage ; absent en création. */
  initialName?: string;
  submitting: boolean;
  error: string | null;
  onSubmit: (name: string) => void;
  onClose: () => void;
}

export default function ExpenseCategoryNameDialog({ initialName, ...rest }: ExpenseCategoryNameDialogProps) {
  return (
    <NameFormDialog
      title={initialName === undefined ? "Ajouter une catégorie" : "Renommer la catégorie"}
      initialName={initialName}
      {...rest}
    />
  );
}
