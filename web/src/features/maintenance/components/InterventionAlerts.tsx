import { Alert } from "@mui/material";

interface InterventionAlertsProps {
  error: string | null;
  reasonRequired: boolean;
  confirmationRequired: boolean;
}

export default function InterventionAlerts({ error, reasonRequired, confirmationRequired }: InterventionAlertsProps) {
  return (
    <>
      {error && (
        <Alert severity="error" sx={{ marginBottom: 2 }}>
          {error}
        </Alert>
      )}
      {reasonRequired && (
        <Alert severity="info" sx={{ marginBottom: 2 }}>
          Ce kilométrage est incohérent avec les relevés voisins : un motif est obligatoire.
        </Alert>
      )}
      {confirmationRequired && (
        <Alert severity="warning" sx={{ marginBottom: 2 }}>
          Plus de 1 000 km/jour en moyenne depuis le relevé précédent. Vérifiez la valeur saisie, puis confirmez pour
          l'enregistrer quand même.
        </Alert>
      )}
    </>
  );
}
