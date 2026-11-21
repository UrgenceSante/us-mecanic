import NameFormDialog from "../../../shared/components/NameFormDialog";

interface VehicleTypeNameDialogProps {
  /** Nom actuel en renommage ; absent en création. */
  initialName?: string;
  submitting: boolean;
  error: string | null;
  onSubmit: (name: string) => void;
  onClose: () => void;
}

export default function VehicleTypeNameDialog({ initialName, ...rest }: VehicleTypeNameDialogProps) {
  return (
    <NameFormDialog
      title={initialName === undefined ? "Ajouter un type" : "Renommer le type"}
      initialName={initialName}
      {...rest}
    />
  );
}
