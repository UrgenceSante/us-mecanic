import { Paper, Typography } from "@mui/material";
import PropertyDisplay from "../../../components/Utils/PropertyDisplay";
import { useReferenceNames } from "../hooks/useReferenceNames";
import type { ProblemAnalysis } from "../types/problem";

/** Analyse en lecture seule (une seule analyse par problème, non modifiable). */
export default function ProblemAnalysisView({ analysis }: { analysis: ProblemAnalysis }) {
  const name = useReferenceNames();
  return (
    <Paper sx={{ padding: 2 }}>
      <Typography variant="h6" marginBottom={1}>
        Analyse
      </Typography>
      <PropertyDisplay title="Nature" content={name("nature", analysis.natureId)} />
      <PropertyDisplay title="Élément" content={name("element", analysis.elementId)} />
      <PropertyDisplay title="Immobilisation" content={analysis.immobilized ? "Véhicule immobilisé" : "Non"} />
      <PropertyDisplay title="Analyse" content={analysis.text} />
    </Paper>
  );
}
