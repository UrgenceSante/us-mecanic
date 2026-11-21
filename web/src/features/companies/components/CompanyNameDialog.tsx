import NameFormDialog from "../../../shared/components/NameFormDialog";

interface CompanyNameDialogProps {
  /** Nom actuel en renommage ; absent en création. */
  initialName?: string;
  submitting: boolean;
  error: string | null;
  onSubmit: (name: string) => void;
  onClose: () => void;
}

export default function CompanyNameDialog({ initialName, ...rest }: CompanyNameDialogProps) {
  return (
    <NameFormDialog
      title={initialName === undefined ? "Ajouter une société" : "Renommer la société"}
      initialName={initialName}
      {...rest}
    />
  );
}
