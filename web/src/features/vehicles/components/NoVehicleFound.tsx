import { Button, Stack, Typography } from "@mui/material";

interface NoVehicleFoundProps {
  /** Vrai quand au moins un filtre est actif : propose de les réinitialiser. */
  filtered: boolean;
  onReset: () => void;
}

/** Liste vide du parc : invite à modifier ou réinitialiser les filtres. */
export default function NoVehicleFound({ filtered, onReset }: NoVehicleFoundProps) {
  return (
    <Stack alignItems="center" gap={1} padding={2}>
      <Typography color="text.secondary">
        {filtered ? "Aucun véhicule trouvé. Modifiez ou réinitialisez les filtres." : "Aucun véhicule trouvé."}
      </Typography>
      {filtered && <Button onClick={onReset}>Réinitialiser</Button>}
    </Stack>
  );
}
