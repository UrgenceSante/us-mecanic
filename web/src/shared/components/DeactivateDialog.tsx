import ConfirmDialog from "./ConfirmDialog";

interface DeactivateDialogProps {
  /** Référentiel avec article défini, ex. « la société » ou « le garage ». */
  definite: string;
  /** Nom de la valeur à désactiver. */
  name: string;
  /** Accorde le texte au féminin (« Elle ne sera plus proposée »). */
  feminine?: boolean;
  submitting: boolean;
  onConfirm: () => void;
  onClose: () => void;
}

/** Confirmation de désactivation d'une valeur de référentiel (pas de réactivation possible depuis l'écran). */
export default function DeactivateDialog({ definite, name, feminine = false, submitting, onConfirm, onClose }: DeactivateDialogProps) {
  return (
    <ConfirmDialog
      title={`Désactiver ${definite}`}
      confirmLabel="Désactiver"
      definitive={false}
      submitting={submitting}
      onConfirm={onConfirm}
      onClose={onClose}
    >
      Désactiver {definite} « {name} » ? {feminine ? "Elle ne sera plus proposée" : "Il ne sera plus proposé"} à la
      saisie.
    </ConfirmDialog>
  );
}
