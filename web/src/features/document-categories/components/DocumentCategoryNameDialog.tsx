import NameFormDialog from "../../../shared/components/NameFormDialog";

interface Props {
  /** Nom actuel en renommage ; absent en création. */
  initialName?: string;
  submitting: boolean;
  /** Erreur de l'API (ex. nom en double), affichée sous le champ. */
  error: string | null;
  onSubmit: (name: string) => void;
  /** Appelé à chaque saisie, pour effacer l'erreur de l'API. */
  onEdit: () => void;
  onClose: () => void;
}

export default function DocumentCategoryNameDialog({ initialName, ...rest }: Props) {
  return (
    <NameFormDialog
      title={initialName === undefined ? "Ajouter une catégorie" : "Renommer la catégorie"}
      initialName={initialName}
      maxLength={100}
      {...rest}
    />
  );
}
