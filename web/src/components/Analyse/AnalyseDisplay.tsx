import { Box, Button, Checkbox, FormControlLabel } from "@mui/material";
import type { IAnalyse } from "./IAnalyse";
import PropertyDisplay from "../Utils/PropertyDisplay";
import AcionsTableDisplay from "./AcionsTableDisplay";
import EditIcon from "@mui/icons-material/Edit";
import { useState } from "react";
// import AnalyseForm from "./AnalyseForm/AnalyseForm";
// import { putAnalyse } from "../../services/Analyse.service";
import SimpleCard from "../Utils/Cards/SimpleCard";

interface AnalyseDisplayProps {
  analyse: IAnalyse;
}

export default function AnalyseDisplay(props: AnalyseDisplayProps) {
  const { analyse } = props;
  // TODO: réactiver l'édition (AnalyseForm + putAnalyse) lors de la réécriture du module Incidents
  const [, setIsEditing] = useState(false);

  const toggleIsEditing = () => setIsEditing((old) => !old);

  // if (isEditing)
  //   return (
  //     <AnalyseForm
  //       mutationFn={putAnalyse}
  //       analyze={analyzeToForm(analyse)}
  //       onCancel={toggleIsEditing}
  //     />
  //   );

  return (
    <>
      <SimpleCard
        title="Analyse"
        action={
          <Button
            onClick={toggleIsEditing}
            color="primary"
            startIcon={<EditIcon />}
          >
            Modifier
          </Button>
        }
      >
        <Box>
          <Box sx={{ display: "flex", flexDirection: "column" }}>
            <FormControlLabel
              control={
                <Checkbox checked={analyse.ImmobilizeVehicle} disabled />
              }
              label="Immobilisation du véhicule nécessaire"
            />
            <Box sx={{ display: "flex", flexDirection: "row", gap: 2 }}>
              <PropertyDisplay
                title="Analysé par"
                content={analyse.AnalyzeBy || "Erreur AnalyzeBy"}
              />
              <PropertyDisplay
                title="Concerne"
                content={analyse.Concerning?.Value || "Erreur Concerning"}
              />
              <PropertyDisplay
                title="Nature"
                content={analyse.Nature?.Value || "Erreur Nature"}
              />
            </Box>
            <PropertyDisplay title="Analyse" content={analyse.Analyze} />
          </Box>
        </Box>
      </SimpleCard>
      <SimpleCard title="Actions">
        <Box>
          <AcionsTableDisplay actions={analyse.Actions} />
        </Box>
      </SimpleCard>
    </>
  );
}
