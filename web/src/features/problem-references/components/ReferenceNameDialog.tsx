import NameFormDialog from "../../../shared/components/NameFormDialog";

interface ReferenceNameDialogProps {
  /** Libellé singulier du référentiel, ex. « nature » ou « type d'action ». */
  singular: string;
  feminine: boolean;
  /** Libellé avec article défini, ex. « l'élément » ou « la nature ». */
  definite: string;
  /** Nom actuel en renommage ; absent en création. */
  initialName?: string;
  submitting: boolean;
  error: string | null;
  onSubmit: (name: string) => void;
  onClose: () => void;
}

export default function ReferenceNameDialog({ singular, feminine, definite, initialName, ...rest }: ReferenceNameDialogProps) {
  const title =
    initialName === undefined ? `Ajouter ${feminine ? "une" : "un"} ${singular}` : `Renommer ${definite}`;
  return <NameFormDialog title={title} initialName={initialName} {...rest} />;
}
