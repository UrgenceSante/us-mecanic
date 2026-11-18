import { Button, Typography } from "@mui/material";
import EditIcon from "@mui/icons-material/Edit";
import type { Company } from "../../companies/types/company";

interface VehicleOwnerLineProps {
  owner: Company | null;
  retired: boolean;
  onFill: () => void;
}

/** Propriétaire dans l'en-tête de la fiche ; sorti du parc sans propriétaire : bouton pour le renseigner. */
export default function VehicleOwnerLine({ owner, retired, onFill }: VehicleOwnerLineProps) {
  if (owner) {
    return <Typography>{`Propriétaire : ${owner.name}${owner.isActive ? "" : " (inactive)"}`}</Typography>;
  }
  return (
    <>
      <Typography color="warning.main">Propriétaire à renseigner</Typography>
      {retired && (
        <div>
          <Button startIcon={<EditIcon />} onClick={onFill}>
            Renseigner le propriétaire
          </Button>
        </div>
      )}
    </>
  );
}
