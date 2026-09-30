import { Box } from "@mui/material";
import AsyncAnalyse from "../components/Analyse/AsyncAnalyse";
import AsyncMecanicLogResume from "../components/MecanicLog/AsyncMecanicLogResume";

import BackToLogsButton from "../components/Utils/Buttons/BackToLogsButton";

export default function AnalyseDetail() {
  return (
    <>
      <BackToLogsButton sx={{ marginY: 2 }} />
      <Box
        sx={{
          display: "grid",
          gap: 3,
          gridTemplateColumns: {
            xs: "1fr", // 1 colonne sur mobile
            md: "repeat(2, 1fr)", // 2 colonnes à partir de 'md' (900px par défaut)
          },
          "& > :nth-of-type(3)": {
            gridColumn: {
              xs: "1 / -1", // toute la ligne, quel que soit le nombre de colonnes
              sm: "span 2",
            },
          },
        }}
      >
        <AsyncMecanicLogResume />

        <AsyncAnalyse />
      </Box>
    </>
  );
}
