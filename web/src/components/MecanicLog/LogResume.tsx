import { Box, Chip } from "@mui/material";
import DateDisplayer from "../Utils/DateDisplayer";
import type { IMecanicLog } from "./IMecanicLog";
import SimpleCard from "../Utils/Cards/SimpleCard";
import PropertyDisplay from "../Utils/PropertyDisplay";
import DirectionsCarIcon from "@mui/icons-material/DirectionsCar";

interface LogResumeProps {
  log: IMecanicLog;
}
export default function LogResume(props: LogResumeProps) {
  const { log } = props;

  return (
    <SimpleCard
      title="Résumé de la déclaration"
      action={
        <Chip
          icon={<DirectionsCarIcon />}
          size="medium"
          label={log.Immatriculation}
        />
      }
    >
      <Box sx={{ display: "flex", gap: 2, flexDirection: "column" }}>
        <Box sx={{display: "flex", flexDirection: "row", gap: 3}}>
          <PropertyDisplay
            title="Date"
            content={<DateDisplayer value={log.LogDate} />}
          />
          <PropertyDisplay title="Ambulancier" content={"Coming soon !"} />
        </Box>

        <PropertyDisplay title="Description" content={log.Report} />
      </Box>
    </SimpleCard>
  );
  //   return (
  //     <Box
  //       sx={{
  //         backgroundColor: "white",
  //         border: "1px solid #ccd0d4",

  //         boxShadow: "0 1px 1px rgba(0, 0, 0, 0.04)",
  //         padding: 2,
  //         minWidth: 275,
  //       }}
  //     >
  //       <Typography marginY={1} variant="h6">
  //         Résumé de la déclaration
  //       </Typography>
  //       <Divider />
  //       <Box>

  //       </Box>
  //       {DataPresenter("Véhicule", log.Immatriculation)}
  //       {DataPresenter("Date", <DateDisplayer value={log.LogDate} />)}
  //       {DataPresenter("Ambulancier", "Not implemented - No Crew Model")}
  //       {DataPresenter("Description", log.Report)}
  //     </Box>
  //   );
}

// const DataPresenter = (label: string, value: string | number | ReactNode) => {
//   return (
//     <Box display="flex" gap={1}>
//       <Typography fontWeight={600}>{label}</Typography>
//       <Typography variant="body1">: {value}</Typography>
//     </Box>
//   );
// };
