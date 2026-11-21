import NameFormDialog from "../../../shared/components/NameFormDialog";

interface GarageNameDialogProps {
  /** Nom actuel en renommage ; absent en création. */
  initialName?: string;
  submitting: boolean;
  error: string | null;
  onSubmit: (name: string) => void;
  onClose: () => void;
}

export default function GarageNameDialog({ initialName, ...rest }: GarageNameDialogProps) {
  return (
    <NameFormDialog
      title={initialName === undefined ? "Ajouter un garage" : "Renommer le garage"}
      initialName={initialName}
      {...rest}
    />
  );
}
