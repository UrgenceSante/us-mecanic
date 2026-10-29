import { Alert, Stack, Tab, Tabs, Typography } from "@mui/material";
import { useState } from "react";
import { problemReferenceConfigs } from "../api/referenceConfigs";
import ReferenceAdminPanel from "../components/ReferenceAdminPanel";

export default function ProblemReferencesPage() {
  const [index, setIndex] = useState(0);
  const config = problemReferenceConfigs[index];

  return (
    <Stack gap={2} marginY={2}>
      <Typography variant="h5">Référentiels des problèmes</Typography>
      <Alert severity="info">
        Une valeur désactivée n'est plus proposée à la saisie mais reste affichée sur les problèmes existants.
      </Alert>
      <Tabs value={index} onChange={(_, value: number) => setIndex(value)} variant="scrollable">
        {problemReferenceConfigs.map((c) => (
          <Tab key={c.kind} label={c.title} />
        ))}
      </Tabs>
      <ReferenceAdminPanel key={config.kind} config={config} />
    </Stack>
  );
}
